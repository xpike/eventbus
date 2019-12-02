using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XPike.Configuration.Microsoft.AspNetCore;
using XPike.Logging.Microsoft.AspNetCore;

namespace XPikeEventBus2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder => { builder.UseXPikeLogging(); }) // NOTE: Call AddXPikeLogging() to preserve any configured NetCore providers.
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.AddXPikeConfiguration(xpike => { })
                        .UseStartup<Startup>();
                });
    }
}