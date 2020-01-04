using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;
using XPike.Logging;

namespace XPike.EventBus.Redis
{
    public class RedisEventBusConnection
        : IRedisEventBusConnection
    {
        private static readonly ConcurrentDictionary<string, ChannelMessageQueue> _handlers = new ConcurrentDictionary<string, ChannelMessageQueue>();

        private readonly ISubscriber _subscriber;
        private readonly ILog<RedisEventBusConnection> _logger;

        public RedisEventBusConnection(ISubscriber subscriber, ILog<RedisEventBusConnection> logger)
        {
            _subscriber = subscriber;
            _logger = logger;
        }

        public async Task<bool> PublishAsync<TMessage>(string targetName,
                                                       TMessage message,
                                                       TimeSpan? timeout = null,
                                                       CancellationToken? ct = null)
            where TMessage : class
        {
            try
            {
                if (await _subscriber.PublishAsync(targetName, JsonConvert.SerializeObject(message))
                                     .ConfigureAwait(false) < 1)
                    _logger.Warn("Message was published to Redis topic with no consumers.",
                                 null,
                                 new Dictionary<string, string>
                                 {
                                     {nameof(targetName), targetName ?? string.Empty},
                                     {nameof(TMessage), typeof(TMessage).FullName}
                                 });

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to publish message to Redis: {ex.Message} ({ex.GetType()})",
                              ex,
                              new Dictionary<string, string>
                              {
                                  {nameof(targetName), targetName ?? string.Empty},
                                  {nameof(TMessage), typeof(TMessage).FullName}
                              });

                return false;
            }
        }

        public async Task<bool> SubscribeAsync<TMessage>(string targetName, Func<TMessage, Task<bool>> asyncHandler,
            TimeSpan? timeout = null, CancellationToken? ct = null)
            where TMessage : class
        {
            if (!_handlers.ContainsKey(targetName))
            {
                var channel = _handlers[targetName] =
                    await _subscriber.SubscribeAsync(targetName).ConfigureAwait(false);

                channel.OnMessage(async message =>
                {
                    await asyncHandler(JsonConvert.DeserializeObject<TMessage>(message.Message.ToString()))
                        .ConfigureAwait(false);
                });
            }

            return true;
        }
    }
}