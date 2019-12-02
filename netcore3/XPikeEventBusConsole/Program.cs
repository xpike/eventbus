using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Example.Library;
using Newtonsoft.Json;
using XPike.Configuration;
using XPike.Configuration.Memory;
using XPike.EventBus;
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
                {"XPike.Logging.ConsoleLogSettings", "{\"Enabled\": true, \"ShowMetadata\": true}"},
                {
                    "XPike.Redis.RedisSettings",
                    "{\"Connections\": { \"default\": { \"ConnectionString\": \"127.0.0.1:6379\", \"Enabled\": true } } }"
                },
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
            container.AddXPikeRedisEventBus();
            container.RegisterSingleton(typeof(ISettings<>), typeof(SettingsLoader<>));

            var provider = container.BuildDependencyProvider();
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
                throw new InvalidOperationException("Failed to subscribe!");

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