using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace XPike.EventBus.RabbitMQ
{
    public interface IRabbitMqEventBusSubscriberConnection
        : IEventBusSubscriberConnection
    {
        Task<IModel> GetSubscriberAsync<TMessage>(string targetName,
                                                  RabbitMqConnectionConfig connectionConfig,
                                                  RabbitMqTargetConfig targetConfig,
                                                  TimeSpan timeout,
                                                  CancellationToken ct);
    }
}