using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using com.timmons.Stitch.Shared;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Amazon.CognitoIdentityProvider;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Newtonsoft.Json;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.AspNetCore.Authorization;
using API.Handlers;
using API.Helpers;
using API.Middleware.Events;

namespace WebAPI
{
    public static class ServiceCollectionExtensions
    {
        static bool forceSignout = false;

        public static IServiceCollection WithDependencyInjection(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddSingleton(Configuration);
            
            return services;
        }

        

    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        
        public void ConfigureServices(IServiceCollection services)
        {
            services
               .WithDependencyInjection(Configuration);

            

            services
                .AddControllers()
                .AddNewtonsoftJson(o =>
                {
                    o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

        }

        


        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            
            
            app.UseRouting();

            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
