using System;
using System.Threading;
using System.Threading.Tasks;

namespace XPike.EventBus
{
    public interface IEventBusConnectionProvider
    {
        Task<IEventBusSubscriberConnection> GetSubscriberConnectionAsync(string connectionName, PublicationType publicationType, TimeSpan? timeout = null, CancellationToken? ct = null);

        Task<IEventBusPublisherConnection> GetPublisherConnectionAsync(string connectionName, PublicationType publicationType, TimeSpan? timeout = null, CancellationToken? ct = null);
    }
}