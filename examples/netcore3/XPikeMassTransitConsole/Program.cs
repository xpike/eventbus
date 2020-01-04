using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Example.Library;
using MassTransit;
using Newtonsoft.Json;

namespace XPikeMassTransitConsole
{
    public class Program
    {
        public static async Task Main()
        {
            var bus = Bus.Factory.CreateUsingRabbitMq(connection =>
                                                      {
                                                          connection.Host("rabbitmq://localhost");

                                                          connection.ReceiveEndpoint("heartbeat-mt",
                                                                                     endpoint =>
                                                                                     {
                                                                                         endpoint.Handler<HeartbeatMessage>(Handler);
                                                                                     });

                                                      });

            await bus.StartAsync();

            Console.WriteLine("Listening...");

            var tasks = new List<Task>();
            for (var i = 0; i < 1000; ++i)
                tasks.Add(bus.Publish(new HeartbeatMessage
                                      {
                                          Origin = nameof(XPikeMassTransitConsole),
                                          Timestamp = DateTime.UtcNow
                                      }));

            await Task.WhenAll(tasks);

            Console.WriteLine("Press any key to exit.");
            await Task.Run(Console.ReadKey);

            await bus.StopAsync();
        }

        private static Task Handler(ConsumeContext<HeartbeatMessage> context) =>
            Console.Out.WriteLineAsync($"Received: {JsonConvert.SerializeObject(context.Message)}");
    }
}