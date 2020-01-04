using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using XPike.Configuration;
using XPike.Logging;

namespace XPike.EventBus.RabbitMQ
{
    public class RabbitMqEventBusConnection
        : IRabbitMqEventBusPublisherConnection,
          IRabbitMqEventBusSubscriberConnection
    {
        public const string _DEFAULT_CONNECTION_NAME = "Default";

        private static readonly object _lockObject = new object();
        private static ConcurrentDictionary<string, string> _configuredTargets = new ConcurrentDictionary<string, string>();
        private static List<IModel> _subscriberChannels = new List<IModel>();
        private static List<IModel> _publisherChannels = new List<IModel>();

        private static readonly BlockingCollection<IModel> _publisherPool = new BlockingCollection<IModel>();

        private readonly string _connectionName;
        private readonly IConfig<RabbitMqConfig> _config;
        private readonly ILog<RabbitMqEventBusConnection> _logger;

        private IConnectionFactory _factory;
        private IConnection _connection;

        public RabbitMqEventBusConnection(string connectionName, IConfig<RabbitMqConfig> config, ILog<RabbitMqEventBusConnection> logger)
        {
            _connectionName = connectionName;
            _config = config;
            _logger = logger;

            CreateConnection();
        }

        private RabbitMqConnectionConfig GetConfiguration(string connectionName)
        {
            connectionName = string.IsNullOrWhiteSpace(connectionName) ? _DEFAULT_CONNECTION_NAME : connectionName;

            if (_config.CurrentValue.Connections.TryGetValue(connectionName,
                                                             out var config))
                return config;

            if (connectionName == _DEFAULT_CONNECTION_NAME)
                throw new InvalidConfigurationException(typeof(RabbitMqConnectionConfig).FullName,
                                                        $"No RabbitMQ configuration found for connection named '{connectionName}'.");

            _logger.Warn($"No RabbitMQ configuration found for connection named '{connectionName}'.  Using '{_DEFAULT_CONNECTION_NAME}' configuration.");
            return GetConfiguration(_DEFAULT_CONNECTION_NAME);
        }

        private RabbitMqQueueConfig GetQueueConfig(RabbitMqConnectionConfig connectionConfig,
                                                   RabbitMqTargetConfig targetConfig)
        {
            if (connectionConfig.Queues.TryGetValue(targetConfig.RoutingKey.Replace(":", "_"), out var config))
                return config;

            return new RabbitMqQueueConfig();
        }

        private RabbitMqExchangeConfig GetExchangeConfig(RabbitMqConnectionConfig connectionConfig,
                                                         RabbitMqTargetConfig targetConfig)
        {
            if (connectionConfig.Exchanges.TryGetValue(targetConfig.Exchange,
                                                       out var config))
                return config;

            return new RabbitMqExchangeConfig
                   {
                       ExchangeType = ExchangeType.Fanout
                   };
        }

        private RabbitMqTargetConfig GetTargetConfig(RabbitMqConnectionConfig connectionConfig, string targetName)
        {
            if (connectionConfig.Targets.TryGetValue(targetName, out var config))
                return config;

            return new RabbitMqTargetConfig
                   {
                       Exchange = string.Empty,
                       RoutingKey = targetName,
                       Enabled = true,
                       ConsumerChannels = 1
                   };
        }

        private void CreateConnection()
        {
            var config = GetConfiguration(_connectionName);

            var protocols = SslProtocols.None;
            if (!string.IsNullOrWhiteSpace(config.SslProtocols))
                foreach (var protocolString in config.SslProtocols.Split(new [] {';', ','}))
                    if (Enum.TryParse<SslProtocols>(protocolString, out var protocol))
                        protocols |= protocol;

            _factory = new ConnectionFactory
                       {
                           Port = config.Port,
                           VirtualHost = config.VirtualHost,
                           AmqpUriSslProtocols = protocols,
                           AutomaticRecoveryEnabled = true,
                           DispatchConsumersAsync = true,
                           HostName = config.Hostname,
                           Password = config.Password,
                           TopologyRecoveryEnabled = true,
                           UseBackgroundThreadsForIO = true,
                           UserName = config.Username,
                           Ssl = new SslOption(config.Hostname,
                                               string.Empty,
                                               protocols != SslProtocols.None)
                       };

            var servers = string.IsNullOrWhiteSpace(config.ClusterMembers) ?
                              new List<string>() :
                              config.ClusterMembers.Split(new[] {';', ','}).ToList();

            if (!servers.Contains(config.Hostname))
                servers.Add(config.Hostname);

            _connection = _factory.CreateConnection(servers);
        }

        private string PrepareTarget(string targetName, IModel channel, RabbitMqConnectionConfig config, RabbitMqTargetConfig targetConfig)
        {
            if (_configuredTargets.TryGetValue(targetName, out var key))
                return key;

            key = targetConfig.RoutingKey;
            var exchangeConfig = GetExchangeConfig(config, targetConfig);

            if (!string.IsNullOrWhiteSpace(targetConfig.Exchange))
                channel.ExchangeDeclare(targetConfig.Exchange,
                                        exchangeConfig.ExchangeType,
                                        exchangeConfig.Durable,
                                        exchangeConfig.AutoDelete);

            if (exchangeConfig.ExchangeType == ExchangeType.Fanout || exchangeConfig.ExchangeType == ExchangeType.Direct)
            {
                var queueConfig = GetQueueConfig(config, targetConfig);

                key = channel.QueueDeclare(targetConfig.RoutingKey,
                                           queueConfig.Durable,
                                           queueConfig.Exclusive,
                                           queueConfig.AutoDelete)
                             .QueueName;

                if (!queueConfig.SkipBinding && !string.IsNullOrWhiteSpace(targetConfig.Exchange))
                    channel.QueueBind(key,
                                      targetConfig.Exchange,
                                      string.IsNullOrWhiteSpace(queueConfig.BindingRoutingKey)
                                          ? string.Empty
                                          : queueConfig.BindingRoutingKey);
            }

            _configuredTargets[targetName] = key;

            return key;
        }

        // TODO: Timeout/cancellation
        public Task<IModel> GetPublisherAsync(string targetName,
                                              RabbitMqConnectionConfig connectionConfig,
                                              RabbitMqTargetConfig targetConfig,
                                              TimeSpan timeout,
                                              CancellationToken ct) =>
            Task.Run(() =>
                     {
                         if (_publisherPool.TryTake(out var publisher))
                             return publisher;

                         if (_publisherChannels.Count < connectionConfig.MaxPublisherChannels)
                         {
                             lock (_lockObject)
                             {
                                 if (_publisherChannels.Count < connectionConfig.MaxPublisherChannels)
                                 {
                                     var channel = _connection.CreateModel();

                                     PrepareTarget(targetName, channel, connectionConfig, targetConfig);

                                     _publisherChannels = new List<IModel>(_publisherChannels)
                                                          {
                                                              channel
                                                          };

                                     _publisherPool.Add(channel);

                                     return channel;
                                 }
                             }
                         }

                         if (_publisherPool.TryTake(out publisher,
                                                    (int) timeout.TotalMilliseconds,
                                                    ct))
                             return publisher;

                         throw new Exception("Timed out acquiring publisher channel.");
                     });

        // TODO: Timeout/cancellation
        public Task<IModel> GetSubscriberAsync<TMessage>(string targetName,
                                                         RabbitMqConnectionConfig connectionConfig,
                                                         RabbitMqTargetConfig targetConfig,
                                                         TimeSpan timeout,
                                                         CancellationToken ct) =>
            Task.Run(() =>
                     {
                         lock (_lockObject)
                         {
                             var channel = _connection.CreateModel();

                             PrepareTarget(targetName, channel, connectionConfig, targetConfig);

                             _subscriberChannels = new List<IModel>(_subscriberChannels)
                                                   {
                                                       channel
                                                   };

                             return channel;
                         }
                     });

        public async Task<bool> PublishAsync<TMessage>(string targetName,
                                                       TMessage message,
                                                       TimeSpan? timeout = null,
                                                       CancellationToken? ct = null)
            where TMessage : class
        {
            var config = GetConfiguration(_connectionName);
            var targetConfig = GetTargetConfig(config, targetName);

            var publisher = await GetPublisherAsync(targetName,
                                                    config,
                                                    targetConfig,
                                                    timeout ?? TimeSpan.FromSeconds(10),
                                                    ct ?? CancellationToken.None);

            try
            {
                // TODO: Support headers, correlation ID and reply-to address at a minimum
                var props = publisher.CreateBasicProperties();
                props.ContentEncoding = "UTF8";
                props.ContentType = "application/json";
                props.Persistent = targetConfig.Persistent;

                await Task.Run(() => publisher.BasicPublish(targetConfig.Exchange,
                                                            targetConfig.RoutingKey,
                                                            targetConfig.Mandatory,
                                                            props,
                                                            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message))));

                return true;
            }
            finally
            {
                _publisherPool.Add(publisher);
            }
        }

        public async Task<bool> SubscribeAsync<TMessage>(string targetName,
                                                         Func<TMessage, Task<bool>> asyncHandler,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken? ct = null)
            where TMessage : class
        {
            var config = GetConfiguration(_connectionName);
            var targetConfig = GetTargetConfig(config, targetName);

            for (var i = 0; i < targetConfig.ConsumerChannels; ++i)
            {
                var subscriber = await GetSubscriberAsync<TMessage>(targetName,
                                                                    config,
                                                                    targetConfig,
                                                                    timeout ?? TimeSpan.FromSeconds(10),
                                                                    ct ?? CancellationToken.None);

                if (targetConfig.PrefetchCount > 0 || targetConfig.PrefetchSize > 0)
                    subscriber.BasicQos((uint) targetConfig.PrefetchSize,
                                        (ushort) targetConfig.PrefetchCount,
                                        targetConfig.GlobalQos);

                var consumer = new AsyncEventingBasicConsumer(subscriber);
                consumer.Received += async (sender, args) =>
                                     {
                                         try
                                         {
                                             var message =
                                                 JsonConvert
                                                     .DeserializeObject<TMessage>(Encoding.UTF8.GetString(args.Body));

                                             if (await asyncHandler(message))
                                             {
                                                 if (!targetConfig.AutoAck)
                                                     subscriber.BasicAck(args.DeliveryTag,
                                                                         false);

                                                 return;
                                             }

                                             _logger.Error("Failed to process message: Handler returned false.",
                                                           null,
                                                           new Dictionary<string, string>
                                                           {
                                                               {nameof(targetName), targetName ?? string.Empty},
                                                               {nameof(TMessage), typeof(TMessage).FullName}
                                                           });
                                         }
                                         catch (Exception ex)
                                         {
                                             _logger.Error($"Failed to process message: {ex.Message} ({ex.GetType()})",
                                                           ex,
                                                           new Dictionary<string, string>
                                                           {
                                                               {nameof(targetName), targetName ?? string.Empty},
                                                               {nameof(TMessage), typeof(TMessage).FullName}
                                                           });
                                         }

                                         if (!targetConfig.AutoAck)
                                             subscriber.BasicNack(args.DeliveryTag,
                                                                  false,
                                                                  targetConfig.RequeueOnFailure);
                                     };

                subscriber.BasicConsume(queue: _configuredTargets[targetName],
                                        autoAck: targetConfig.AutoAck,
                                        consumer: consumer);
            }

            return true;
        }
    }
}