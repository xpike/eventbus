using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Example.Library;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XPike.EventBus;
using XPike.Logging;

namespace XPikeEventBus.Controllers
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
            if (await _service.PublishAsync(null,
                "test",
                new TestMessage
                {
                    Message = message,
                    Created = DateTime.UtcNow,
                    MessageId = Guid.NewGuid(),
                    Source = "XPikeEventBus"
                },
                PublicationType.BroadcastEvent))
                return Ok();

            return Problem("Failed to publish message.");
        }
    }
}
