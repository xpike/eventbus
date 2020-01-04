using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using XPike.Logging;

namespace XPike.EventBus.MassTransit.RabbitMQ
{
    public class MtxRmqEventBusConnectionProvider
        : IMtxRmqEventBusConnectionProvider
    {
        private readonly ILog<MtxRmqEventBusSubscriberConnection> _subscriberLogger;
        private readonly ILog<MtxRmqEventBusPublisherConnection> _publisherLogger;
        private readonly IBusControl _busControl;
        private readonly IPublishEndpoint _publishEndpoint;

        public MtxRmqEventBusConnectionProvider(ILog<MtxRmqEventBusSubscriberConnection> subscriberLogger,
                                                ILog<MtxRmqEventBusPublisherConnection> publisherLogger,
                                                IBusControl busControl,
                                                IPublishEndpoint publishEndpoint)
        {
            _subscriberLogger = subscriberLogger;
            _publisherLogger = publisherLogger;
            _busControl = busControl;
            _publishEndpoint = publishEndpoint;
        }

        public Task<IEventBusSubscriberConnection> GetSubscriberConnectionAsync(string connectionName,
                                                                                PublicationType publicationType,
                                                                                TimeSpan? timeout = null,
                                                                                CancellationToken? ct = null) =>
            Task.FromResult<IEventBusSubscriberConnection>(new MtxRmqEventBusSubscriberConnection(_subscriberLogger, _busControl));

        public Task<IEventBusPublisherConnection> GetPublisherConnectionAsync(string connectionName,
                                                                              PublicationType publicationType,
                                                                              TimeSpan? timeout = null,
                                                                              CancellationToken? ct = null) =>
            Task.FromResult<IEventBusPublisherConnection>(new MtxRmqEventBusPublisherConnection(_publishEndpoint, _publisherLogger));
    }
}