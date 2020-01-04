using System;
using System.Threading;
using System.Threading.Tasks;

namespace XPike.EventBus
{
    public interface IEventBusSubscriberConnection
    {
        Task<bool> SubscribeAsync<TMessage>(string targetName,
            Func<TMessage, Task<bool>> asyncHandler,
            TimeSpan? timeout = null,
            CancellationToken? ct = null)
            where TMessage : class;
    }
}