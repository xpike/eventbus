using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Example.Library;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XPike.EventBus;
using XPike.Logging;

namespace XPikeMassTransit.Controllers
{
    [ApiController]
    [Route("")]
    public class TestController
        : ControllerBase
    {
        private readonly ILog<TestController> _logger;
        private readonly IEventBusService _service;

        public TestController(ILog<TestController> logger, IEventBusService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet("publish")]
        public async Task<IActionResult> Get([FromQuery] string message)
        {
            var sw = Stopwatch.StartNew();
            var rabbit = new List<Task<bool>>();
            
            for (var i = 0; i < 1000; ++i)
                rabbit.Add(_service.PublishAsync("heartbeat",
                                                 "heartbeatPublish",
                                                 new HeartbeatMessage
                                                 {
                                                     Origin = Dns.GetHostName(),
                                                     Timestamp = DateTime.UtcNow
                                                 },
                                                 PublicationType.BroadcastEvent));

            if (!(await Task.WhenAll(rabbit)).All(x => x))
                return Problem("Failed to publish message to Rabbit.");

            _logger.Info($"Published to RabbitMQ in {sw.Elapsed.TotalMilliseconds}ms.");

            await Task.Delay(TimeSpan.FromSeconds(10));

            sw = Stopwatch.StartNew();

            var redis = new List<Task<bool>>();

            for (var i = 0; i < 1000; ++i)
                redis.Add(_service.PublishAsync(null,
                                                "test",
                                                new TestMessage
                                                {
                                                    Message = message,
                                                    Created = DateTime.UtcNow,
                                                    MessageId = Guid.NewGuid(),
                                                    Source = "XPikeEventBus"
                                                },
                                                PublicationType.BroadcastEvent));

            if (!(await Task.WhenAll(redis)).All(x => x))
                return Problem("Failed to publish message to Redis.");

            _logger.Info($"Published to Redis in {sw.Elapsed.TotalMilliseconds}ms.");

            return Ok();
        }
    }
}
