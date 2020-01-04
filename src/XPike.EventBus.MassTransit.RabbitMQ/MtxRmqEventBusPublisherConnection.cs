using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using XPike.Logging;

namespace XPike.EventBus.MassTransit.RabbitMQ
{
    public class MtxRmqEventBusPublisherConnection : IMtxRmqEventBusPublisherConnection
    {
        private readonly IPublishEndpoint _publisher;
        private readonly ILog<MtxRmqEventBusPublisherConnection> _logger;

        public MtxRmqEventBusPublisherConnection(IPublishEndpoint publisher,
                                                 ILog<MtxRmqEventBusPublisherConnection> logger)
        {
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<bool> PublishAsync<TMessage>(string targetName,
                                                       TMessage message,
                                                       TimeSpan? timeout = null,
                                                       CancellationToken? ct = null)
            where TMessage : class
        {
            try
            {
                await _publisher.Publish<TMessage>(message,
                                                   ct ?? CancellationToken.None);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to publish: {ex.Message} ({ex.GetType()})",
                              ex,
                              new Dictionary<string, string>
                              {
                                  {nameof(TMessage), typeof(TMessage).FullName}
                              });

                return false;
            }
        }
    }
}