using System;
using System.Threading.Tasks;
using Example.Library;
using MassTransit;
using Newtonsoft.Json;
using XPike.Configuration;
using XPike.Logging;
using XPike.Logging.Console;

namespace XPikeMassTransitCoreService
{
    public class HeartbeatConsumer
        : IConsumer<HeartbeatMessage>
    {
        private readonly IConfig<ConsoleLogConfig> _config;
        private readonly ILog<HeartbeatConsumer> _logger;
        private readonly ITestDependency _testDependency;

        private readonly Random _rnd = new Random((int) DateTime.UtcNow.Ticks);

        public HeartbeatConsumer(IConfig<ConsoleLogConfig> config, ILog<HeartbeatConsumer> logger, ITestDependency testDependency)
        {
            _config = config;
            _logger = logger;
            _testDependency = testDependency;
        }

        public async Task Consume(ConsumeContext<HeartbeatMessage> context)
        {
            var msg = $"Received ({_config.CurrentValue.Enabled}): {JsonConvert.SerializeObject(context.Message)}";

            if (_rnd.Next(0, 100) > 50)
                await _testDependency.FakeActivityAsync();

            await Console.Out.WriteLineAsync(msg);
            _logger.Info(msg);
        }
    }
}