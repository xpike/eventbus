using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace XPike.EventBus.Redis
{
    public class RedisEventBusConnection
        : IRedisEventBusConnection
    {
        private static readonly ConcurrentDictionary<string, ChannelMessageQueue> _handlers = new ConcurrentDictionary<string, ChannelMessageQueue>();
        private readonly ISubscriber _subscriber;

        public RedisEventBusConnection(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        public async Task<bool> PublishAsync<TMessage>(string targetName, TMessage message, TimeSpan? timeout = null, CancellationToken? ct = null) =>
            await _subscriber.PublishAsync(targetName, JsonConvert.SerializeObject(message)) > 0;

        public async Task<bool> SubscribeAsync<TMessage>(string targetName, Func<TMessage, Task<bool>> asyncHandler, TimeSpan? timeout = null, CancellationToken? ct = null)
        {
            if (!_handlers.ContainsKey(targetName))
            {
                var channel = _handlers[targetName] = await _subscriber.SubscribeAsync(targetName);

                channel.OnMessage(async message =>
                {
                    await asyncHandler(JsonConvert.DeserializeObject<TMessage>(message.Message.ToString()));
                });
            }

            return true;
        }

    }
}