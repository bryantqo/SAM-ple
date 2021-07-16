using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.timmons.cognitive.API.Util;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Init.init();

            if (args.Length > 0)
            {

                try
                {
                    CreateHostBuilderWithSSM(args).Build().Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to start the web service with ssm. Encountered an exception.");
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("Attempting to start without params. Note it probably wont work correctly.");
                    CreateHostBuilder(args).Build().Run();
                }
            }
            else
                CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static IHostBuilder CreateHostBuilderWithSSM(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddSystemsManager(args[0]);
                builder.AddStitchConfig();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
