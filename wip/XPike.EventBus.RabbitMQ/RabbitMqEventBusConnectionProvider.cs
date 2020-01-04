using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XPike.Configuration;
using XPike.Logging;

namespace XPike.EventBus.RabbitMQ
{
    public class RabbitMqEventBusConnectionProvider
        : IRabbitMqEventBusConnectionProvider
    {
        private static readonly ConcurrentDictionary<string, RabbitMqEventBusConnection> _connections =
            new ConcurrentDictionary<string, RabbitMqEventBusConnection>();

        private readonly IConfig<RabbitMqConfig> _config;
        private readonly ILog<RabbitMqEventBusConnectionProvider> _logger;
        private readonly ILog<RabbitMqEventBusConnection> _connectionLogger;

        public RabbitMqEventBusConnectionProvider(IConfig<RabbitMqConfig> config,
                                                  ILog<RabbitMqEventBusConnectionProvider> logger,
                                                  ILog<RabbitMqEventBusConnection> connectionLogger)
        {
            _config = config;
            _logger = logger;
            _connectionLogger = connectionLogger;
        }

        // TODO: Timeout / cancellation
        protected virtual Task<RabbitMqEventBusConnection> GetConnectionAsync(string connectionName,
                                                                              PublicationType publicationType,
                                                                              TimeSpan? timeout = null,
                                                                              CancellationToken? ct = null) =>
            Task.Run(() => _connections.GetOrAdd(connectionName,
                                                 _ => new RabbitMqEventBusConnection(connectionName,
                                                                                     _config,
                                                                                     _connectionLogger)));

        public virtual async Task<IEventBusSubscriberConnection> GetSubscriberConnectionAsync(string connectionName,
                                                                                              PublicationType publicationType,
                                                                                              TimeSpan? timeout = null,
                                                                                              CancellationToken? ct = null)
        {
            try
            {
                return await GetConnectionAsync(connectionName,
                                                publicationType,
                                                timeout,
                                                ct);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to establish RabbitMQ connection: {ex.Message} ({ex.GetType()})",
                              ex,
                              new Dictionary<string, string>
                              {
                                  {nameof(connectionName), connectionName ?? string.Empty},
                                  {nameof(publicationType), publicationType.ToString()}
                              });

                throw;
            }
        }

        public virtual async Task<IEventBusPublisherConnection> GetPublisherConnectionAsync(string connectionName,
                                                                                            PublicationType publicationType,
                                                                                            TimeSpan? timeout = null,
                                                                                            CancellationToken? ct = null)
        {
            try
            {
                return await GetConnectionAsync(connectionName,
                                                publicationType,
                                                timeout,
                                                ct);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to establish RabbitMQ connection: {ex.Message} ({ex.GetType()})",
                              ex,
                              new Dictionary<string, string>
                              {
                                  {nameof(connectionName), connectionName ?? string.Empty},
                                  {nameof(publicationType), publicationType.ToString()}
                              });

                throw;
            }
        }
    }
}