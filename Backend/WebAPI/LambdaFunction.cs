using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace WebAPI
{
    public class LambdaFunction : Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    {
        protected override void Init(IWebHostBuilder builder)
        {
            _ = builder
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration(builder =>
                {
                    var pth = Environment.GetEnvironmentVariable("ssmpath");

                    try
                    {
                        builder.AddSystemsManager(pth);
                        Console.WriteLine("Added Systems Manager SSM with a base path of " + pth);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Unable to add SSM config. Check that this has the right permission and the path exists! " + pth);
                        Console.WriteLine(ex);
                    }

                    try
                    {
                        com.timmons.cognitive.API.Util.Init.init();
                        builder.AddStitchConfig();
                        Console.WriteLine("Stitch config added!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unable to add stitch config. Is the connection string correct and database visible?");
                        Console.WriteLine(ex);
                    }
                })
                .UseStartup<Startup>()
                .UseLambdaServer();
        }
    }
}
