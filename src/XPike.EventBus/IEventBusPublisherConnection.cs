using System;
using System.Threading;
using System.Threading.Tasks;

namespace XPike.EventBus
{
    public interface IEventBusPublisherConnection
    {
        Task<bool> PublishAsync<TMessage>(string targetName, TMessage message, TimeSpan? timeout = null,
            CancellationToken? ct = null)
            where TMessage : class;
    }
}