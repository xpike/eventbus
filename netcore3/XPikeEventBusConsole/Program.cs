using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Example.Library;
using Newtonsoft.Json;
using XPike.Configuration;
using XPike.Configuration.Memory;
using XPike.EventBus;
using XPike.EventBus.MassTransit.RabbitMQ;
using XPike.EventBus.RabbitMQ;
using XPike.EventBus.Redis;
using XPike.IoC;
using XPike.IoC.SimpleInjector;
using XPike.Settings;
using XPike.Settings.Managers;

namespace XPikeEventBusConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var task = Task.Run(async () => await MainAsync(args));
            Console.WriteLine("Launched...");

            task.Wait();

            Console.WriteLine("Terminated.");
        }

        static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Hello World!");
            await Task.Delay(500);

            var container = new SimpleInjectorDependencyCollection();
            container.LoadPackage(new XPike.Configuration.Package(new Dictionary<string, string>
            {
                {"XPike.Logging.Console.ConsoleLogConfig", "{\"Enabled\": true, \"ShowMetadata\": false}"},
                {
                    "XPike.Redis.RedisConfig",
                    "{\"Connections\": { \"default\": { \"ConnectionString\": \"127.0.0.1:6379\", \"Enabled\": true } } }"
                },
                {
                    "XPike.EventBus.RabbitMQ.RabbitMqConfig",
                    "{\r\n          \"Connections\": {\r\n            \"heartbeat\": {\r\n              \"Hostname\": \"localhost\",\r\n              \"Port\": 5672,\r\n              \"VirtualHost\": \"/\",\r\n              \"Username\": \"guest\",\r\n              \"Password\": \"guest\",\r\n              \"PurgeOnStartup\": false,\r\n              \"ClusterMembers\": \"localhost\",\r\n              \"PublisherConfirmation\": true,\r\n              \"SslProtocols\": \"None\",\r\n              \"Targets\": {\r\n                \"heartbeatPublish\": {\r\n                  \"Exchange\": \"heartbeat\",\r\n                  \"RoutingKey\": \"heartbeat\",\r\n                  \"Persistent\": true,\r\n                  \"AutoAck\": false,\r\n                  \"PrefetchSize\": 10,\r\n                  \"PrefetchCount\": 10,\r\n                  \"GlobalQos\": false,\r\n                  \"Mandatory\": true,\r\n                  \"Enabled\": true,\r\n                  \"ConsumerChannels\": 8,\r\n                  \"RequeueOnFailure\": true\r\n                },\r\n                \"heartbeatConsume\": {\r\n                  \"Exchange\": \"heartbeat\",\r\n                  \"RoutingKey\": \"heartbeat\",\r\n                  \"Persistent\": true,\r\n                  \"AutoAck\": false,\r\n                  \"PrefetchSize\": 0,\r\n                  \"PrefetchCount\": 1,\r\n                  \"GlobalQos\": false,\r\n                  \"Mandatory\": true,\r\n                  \"Enabled\": true,\r\n                  \"ConsumerChannels\": 8,\r\n                  \"RequeueOnFailure\": true\r\n                }\r\n, \"heartbeatTopic\": {\r\n                  \"Exchange\": \"heartbeat\",\r\n                  \"RoutingKey\": \"\",\r\n                  \"Persistent\": true,\r\n                  \"AutoAck\": false,\r\n                  \"PrefetchSize\": 0,\r\n                  \"PrefetchCount\": 1,\r\n                  \"GlobalQos\": false,\r\n                  \"Mandatory\": true,\r\n                  \"Enabled\": true,\r\n                  \"ConsumerChannels\": 8,\r\n                  \"RequeueOnFailure\": true\r\n                }\r\n              },\r\n              \"Exchanges\": {\r\n                \"heartbeat\": {\r\n                  \"Durable\": true,\r\n                  \"AutoDelete\": false,\r\n                  \"ExchangeType\": \"fanout\"\r\n                }\r\n              },\r\n              \"Queues\": {\r\n                \"heartbeat\": {\r\n                  \"Durable\": true,\r\n                  \"AutoDelete\": false,\r\n                  \"Exclusive\": false,\r\n                  \"SkipBinding\": false,\r\n                  \"BindingRoutingKey\": \"\"\r\n                }\r\n              },\r\n              \"Enabled\": true,\r\n              \"MaxPublisherChannels\": 4\r\n            } \r\n          } \r\n        }"
                }
            }));

            //container.AddXPikeConfiguration(xpike =>
            //{
            //    xpike.AddProvider(new MemoryConfigurationProvider(new Dictionary<string, string>
            //    {
            //        {"XPike.Logging.ConsoleLogSettings", "{\"Enabled\": true, \"ShowMetadata\": true}"},
            //        {"XPike.Redis.RedisSettings", "{\"Connections\": { \"default\": { \"ConnectionString\": \"127.0.0.1:6379\", \"Enabled\": true } } }"},
            //    }));
            //});

            container.LoadPackage(new XPike.Logging.Package());
            container.AddXPikeRabbitMqEventBus();
            container.AddXPikeRedisEventBus();
            container.RegisterSingleton(typeof(ISettings<>), typeof(SettingsLoader<>));

            var provider = container.BuildDependencyProvider(); 
            provider.UseXPikeEventBusProvider<IRabbitMqEventBusConnectionProvider>("heartbeat");
            var bus = provider.ResolveDependency<IEventBusService>();
            var service = provider.ResolveDependency<IConfigurationService>();

            if (!await bus.SubscribeAsync<TestMessage>(null,
                                                       "test",
                                                       message =>
                                                       {
                                                           Console.WriteLine(JsonConvert.SerializeObject(message));
                                                           return Task.FromResult(true);
                                                       },
                                                       PublicationType.BroadcastEvent))
                throw new InvalidOperationException("Failed to subscribe to Redis!");

            if (!await bus.SubscribeAsync<HeartbeatMessage>("heartbeat",
                                                            "heartbeatConsume",
                                                            message =>
                                                            {
                                                                Console.WriteLine($"As Queue: {JsonConvert.SerializeObject(message)}");
                                                                return Task.FromResult(true);
                                                            },
                                                            PublicationType.EnqueueCommand))
                throw new InvalidOperationException("Failed to subscribe to RabbitMQ!");

            if (!await bus.SubscribeAsync<HeartbeatMessage>("heartbeat",
                                                            "heartbeatTopic",
                                                            message =>
                                                            {
                                                                Console.WriteLine($"As Topic: {JsonConvert.SerializeObject(message)}");
                                                                return Task.FromResult(true);
                                                            },
                                                            PublicationType.BroadcastEvent))
                throw new InvalidOperationException("Failed to subscribe to RabbitMQ!");

            Console.WriteLine("Press [space] to stop listening.");

            while (true)
            {
                switch(Console.ReadKey(true).Key)
                {
                    case ConsoleKey.Spacebar:
                        return;
                    default:
                        break;
                }

                await Task.Delay(500);
            }
        }
    }
}