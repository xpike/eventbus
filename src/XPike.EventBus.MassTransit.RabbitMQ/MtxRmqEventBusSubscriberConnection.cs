using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using XPike.Logging;

namespace XPike.EventBus.MassTransit.RabbitMQ
{
    public class MtxRmqEventBusSubscriberConnection
        : IMtxRmqEventBusSubscriberConnection
    {
        private readonly ILog<MtxRmqEventBusSubscriberConnection> _logger;
        private readonly IBusControl _busControl;

        public MtxRmqEventBusSubscriberConnection(ILog<MtxRmqEventBusSubscriberConnection> logger, IBusControl busControl)
        {
            _logger = logger;
            _busControl = busControl;
        }

        public async Task<bool> SubscribeAsync<TMessage>(string targetName, 
            Func<TMessage, Task<bool>> asyncHandler, 
            TimeSpan? timeout = null, 
            CancellationToken? ct = null)
            where TMessage : class
        {
            var endpoint = _busControl.ConnectReceiveEndpoint(targetName,
                                                              cfg =>
                                                              {
                                                                  cfg.Handler<TMessage>(async context =>
                                                                                        {
                                                                                            if (!await asyncHandler(context.Message))
                                                                                            {
                                                                                                _logger.Error("Failed to process message: Handler returned false.",
                                                                                                           null,
                                                                                                           new
                                                                                                           Dictionary<string, string> 
                                                                                                           {
                                                                                                               { nameof(TMessage), typeof(TMessage).FullName }
                                                                                                           });
                                                                                                
                                                                                                throw new Exception("Failed to process message: Handler returned false.");
                                                                                            }
                                                                                        });
                                                              });
            
            await endpoint.Ready;

            return true;
        }
    }
}