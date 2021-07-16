using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace WebAPI
{
    /// <summary>
    /// This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the 
    /// actual Lambda function entry point. The Lambda handler field should be set to
    /// 
    /// API::API.LambdaEntryPoint::FunctionHandlerAsync
    /// </summary>
    public class LambdaEntryPoint :

        // The base class must be set to match the AWS service invoking the Lambda function. If not Amazon.Lambda.AspNetCoreServer
        // will fail to convert the incoming request correctly into a valid ASP.NET Core request.
        //
        // API Gateway REST API                         -> Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
        // API Gateway HTTP API payload version 1.0     -> Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
        // API Gateway HTTP API payload version 2.0     -> Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction
        // Application Load Balancer                    -> Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction
        // 
        // Note: When using the AWS::Serverless::Function resource with an event type of "HttpApi" then payload version 2.0
        // will be the default and you must make Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction the base class.

        Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    {
        /// <summary>
        /// The builder has configuration, logging and Amazon API Gateway already configured. The startup class
        /// needs to be configured in this method using the UseStartup<>() method.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IWebHostBuilder builder)
        {
            Console.WriteLine("Lambda Start");
            //try
            //{
            //    var ssmPath = Environment.GetEnvironmentVariable("SSMPath");

            //    Console.WriteLine("SSM Path set to " + ssmPath);

            //    if (ssmPath != null)
            //        builder
            //        .ConfigureAppConfiguration(builder =>
            //        {
            //            builder.AddSystemsManager(ssmPath);
            //        });
            //    else
            //        Console.WriteLine("SSMPath was null. Starting without SSM.");
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine("Unable to start with SSM. Did SSMPath get set?");
            //    Console.WriteLine(ex);
            //    Console.WriteLine(ex.StackTrace);

            //}

            builder
            .UseStartup<Startup>();
        }

        /// <summary>
        /// Use this override to customize the services registered with the IHostBuilder. 
        /// 
        /// It is recommended not to call ConfigureWebHostDefaults to configure the IWebHostBuilder inside this method.
        /// Instead customize the IWebHostBuilder in the Init(IWebHostBuilder) overload.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IHostBuilder builder)
        {
        }
    }
}
