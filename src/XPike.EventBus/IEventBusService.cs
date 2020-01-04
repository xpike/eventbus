using System;
using System.Threading;
using System.Threading.Tasks;

namespace XPike.EventBus
{
    public interface IEventBusService
    {
        bool AddConnectionProvider(string connectionName, IEventBusConnectionProvider provider);

        Task<bool> PublishAsync<TMessage>(string connectionName,
            string targetName,
            TMessage message,
            PublicationType publicationType,
            TimeSpan? timeout = null,
            CancellationToken? ct = null)
            where TMessage : class;

        Task<bool> SubscribeAsync<TMessage>(string connectionName,
            string targetName,
            Func<TMessage, Task<bool>> asyncHandler,
            PublicationType publicationType,
            TimeSpan? timeout = null,
            CancellationToken? ct = null)
            where TMessage : class;
    }
}