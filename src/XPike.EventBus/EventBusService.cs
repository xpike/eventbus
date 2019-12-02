using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace XPike.EventBus
{
    public class EventBusService
        : IEventBusService
    {
        private readonly ConcurrentDictionary<string, IEventBusConnectionProvider> _providers = new ConcurrentDictionary<string, IEventBusConnectionProvider>();
        private readonly IEventBusConnectionProvider _defaultProvider;

        public EventBusService(IEventBusConnectionProvider defaultProvider)
        {
            _defaultProvider = defaultProvider;
        }

        public bool AddConnectionProvider(string connectionName, IEventBusConnectionProvider provider) =>
            _providers.TryAdd(string.IsNullOrWhiteSpace(connectionName) ? "default" : connectionName, provider);

        public async Task<bool> PublishAsync<TMessage>(string connectionName, 
            string targetName, 
            TMessage message, 
            PublicationType publicationType, 
            TimeSpan? timeout = null, 
            CancellationToken? ct = null)
        {
            connectionName = string.IsNullOrWhiteSpace(connectionName) ? "default" : connectionName;

            if (!_providers.TryGetValue(connectionName, out var provider))
                provider = _defaultProvider;

            var connection = await provider.GetPublisherConnectionAsync(connectionName, publicationType, timeout, ct);
            return await connection.PublishAsync(targetName, message, timeout, ct);
        }

        public async Task<bool> SubscribeAsync<TMessage>(string connectionName, 
            string targetName, 
            Func<TMessage, Task<bool>> asyncHandler, 
            PublicationType publicationType,
            TimeSpan? timeout = null,
            CancellationToken? ct = null)
        {
            connectionName = string.IsNullOrWhiteSpace(connectionName) ? "default" : connectionName;

            if (!_providers.TryGetValue(connectionName, out var provider))
                provider = _defaultProvider;

            var connection = await provider.GetSubscriberConnectionAsync(connectionName, publicationType, timeout, ct);
            return await connection.SubscribeAsync(targetName, asyncHandler, timeout, ct);
        }
    }
}