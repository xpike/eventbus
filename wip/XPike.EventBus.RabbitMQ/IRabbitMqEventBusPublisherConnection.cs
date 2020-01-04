using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace XPike.EventBus.RabbitMQ
{
    public interface IRabbitMqEventBusPublisherConnection
        : IEventBusPublisherConnection
    {
        Task<IModel> GetPublisherAsync(string targetName,
                                       RabbitMqConnectionConfig connectionConfig,
                                       RabbitMqTargetConfig targetConfig,
                                       TimeSpan timeout,
                                       CancellationToken ct);
    }
}