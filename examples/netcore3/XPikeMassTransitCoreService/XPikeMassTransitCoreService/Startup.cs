using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Example.Library;
using GreenPipes;
using MassTransit;
using MassTransit.AspNetCoreIntegration;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using XPike.Configuration;
using XPike.Configuration.Microsoft.AspNetCore;
using XPike.EventBus;
using XPike.EventBus.MassTransit.RabbitMQ;
using XPike.IoC.Microsoft.AspNetCore;
//using XPike.IoC.SimpleInjector.AspNetCore;
using XPike.Logging.Microsoft.AspNetCore;

namespace XPikeMassTransitCoreService
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            services.AddMvc();

            services.UseMicrosoftConfigurationForXPike();

            services.AddMassTransit(CreateBus, ConfigureBus);

            services.AddXPikeDependencyInjection()
                    .AddXPikeMassTransitRabbitMqEventBus()
                    .AddXPikeLogging()
                    .RegisterScoped<ITestDependency, TestDependency>();
        }

        private void ConfigureBus(IServiceCollectionConfigurator configurator)
        {
            // NOTE: Un-comment this to use traditional MassTransit Consumer
            //configurator.AddConsumer<HeartbeatConsumer>();
        }

        private IBusControl CreateBus(IServiceProvider provider)
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
                                                   {
                                                       cfg.Host("rabbitmq://localhost");

                                                       // NOTE: Un-comment this to use traditional MassTransit Consumer
                                                       //cfg.ReceiveEndpoint("heartbeat-mt",
                                                       //                    endpoint =>
                                                       //                    {
                                                       //                        endpoint.PrefetchCount = 10;
                                                       //                        endpoint.UseMessageRetry(r => r.Interval(2, 100));
                                                       //                        endpoint.ConfigureConsumer<HeartbeatConsumer>(provider);
                                                       //                    });
                                                   });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var xpike = app.UseXPikeDependencyInjection();
            xpike.UseXPikeLogging();

            var bus = xpike.ResolveDependency<IEventBusService>();

            // NOTE: Comment this out to use traditional MassTransit Consumer
            bus.SubscribeAsync<HeartbeatMessage>(null,
                                                        "heartbeat-mt",
                                                        async message =>
                                                        {
                                                            await Console.Out.WriteLineAsync($"Received! {JsonConvert.SerializeObject(message)}");
                                                            return true;
                                                        },
                                                        PublicationType.BroadcastEvent)
                      .GetAwaiter()
                      .GetResult();

            bus.PublishAsync<HeartbeatMessage>(null,
                                               "heartbeat-mt",
                                               new HeartbeatMessage
                                               {
                                                   Timestamp = DateTime.UtcNow,
                                                   Origin = nameof(XPikeMassTransitCoreService)
                                               },
                                               PublicationType.BroadcastEvent)
               .GetAwaiter()
               .GetResult();

            app.UseHealthChecks("/health",
                                new HealthCheckOptions
                                {
                                    Predicate = check => check.Tags.Contains("ready")
                                });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
