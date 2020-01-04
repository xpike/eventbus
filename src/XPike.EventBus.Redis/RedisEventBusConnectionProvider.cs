using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using XPike.Logging;
using XPike.Redis;

namespace XPike.EventBus.Redis
{
    public class RedisEventBusConnectionProvider
        : IRedisEventBusConnectionProvider
    {
        private static readonly ConcurrentDictionary<string, IRedisEventBusConnection> _connections =
            new ConcurrentDictionary<string, IRedisEventBusConnection>();

        private readonly IRedisConnectionProvider _provider;
        private readonly ILog<RedisEventBusConnection> _connectionLogger;

        public RedisEventBusConnectionProvider(IRedisConnectionProvider provider, ILog<RedisEventBusConnection> connectionLogger)
        {
            _provider = provider;
            _connectionLogger = connectionLogger;
        }

        private async Task<IRedisEventBusConnection> GetConnectionAsync(string connectionName,
            PublicationType publicationType,
            TimeSpan? timeout = null,
            CancellationToken? ct = null)
        {
            if (publicationType != PublicationType.BroadcastEvent)
                throw new InvalidOperationException("XPike.EventBus.Redis only supports PublicationType.BroadcastEvent");

            connectionName = string.IsNullOrWhiteSpace(connectionName) ? "default" : connectionName;
            return _connections.TryGetValue(connectionName, out var subscriber) ?
                       subscriber :
                       _connections[connectionName] = new RedisEventBusConnection(await (await _provider.GetConnectionAsync(connectionName, timeout, ct)
                                                                                                        .ConfigureAwait(false))
                                                                                        .GetSubscriberAsync()
                                                                                        .ConfigureAwait(false),
                                                                                  _connectionLogger);
        }

        public async Task<IEventBusSubscriberConnection> GetSubscriberConnectionAsync(string connectionName,
            PublicationType publicationType,
            TimeSpan? timeout = null,
            CancellationToken? ct = null) =>
            await GetConnectionAsync(connectionName, publicationType, timeout, ct).ConfigureAwait(false);

        public async Task<IEventBusPublisherConnection> GetPublisherConnectionAsync(string connectionName,
            PublicationType publicationType, 
            TimeSpan? timeout = null,
            CancellationToken? ct = null) =>
            await GetConnectionAsync(connectionName, publicationType, timeout, ct).ConfigureAwait(false);
    }
}