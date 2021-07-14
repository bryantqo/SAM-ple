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

namespace API
{
    public static class ServiceCollectionExtensions
    {
        static bool forceSignout = false;

        public static IServiceCollection WithDependencyInjection(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddSingleton(Configuration);
            services.AddSingleton<ISecrets, EnvSecrets>();
            services.AddSingleton<IConnection, AppConnection>();
            services.AddSingleton<IMapServiceProvider, MapServiceProvider>();
            services.AddSingleton<IACLMapper, PAMACLMapper>();
            services.AddSingleton<IAppConfigMapper, PAMAppConfigMapper>();
            services.AddSingleton<IOrganizationMapper, PAMOrganizationMapper>();
            services.AddSingleton<IAuthorizationPolicyProvider, HasACLPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, HasACLHandler>();

            services.AddSingleton<UsersHelper, UsersHelper>();
            services.AddSingleton<GenericConfigHelper, GenericConfigHelper>();

            services.AddSingleton<IEventStore, PAMEventStore>();

            return services;
        }

        public static IServiceCollection WithDevelopmentCORS(this IServiceCollection services)
        {
            try
            {
                string[] origins =
                {
                    "localhost",
                    "localhost:44358",
                    "localhost:44359",
                    "localhost:3000",
                    "localhost:8080",
                    "localhost:8000"
                };

                List<string> allOrigins = new List<string>();

                foreach(var origin in origins)
                {
                    allOrigins.Add("http://" + origin);
                    allOrigins.Add("https://" + origin);
                }

                
                services.AddCors(options =>
                {
                    options.AddPolicy("DevelopmentCORS", builder =>
                    {

                        builder
                            .WithOrigins(allOrigins.ToArray())
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();

                    });
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to add CORS");
                Console.WriteLine(ex);
            } 

            return services;
        }

        public static IServiceCollection WithRuntimeCORS(this IServiceCollection services, IConfiguration Configuration)
        {
            try
            {
                
                List<string> allOrigins = new List<string>();

                int idx = 0;

                //This is hacky but meh
                var origin = Configuration.GetValue<string>("CORS:" + idx);
                while(origin != null)
                {
                    allOrigins.Add("https://" + origin);
                    idx++;
                    origin = Configuration.GetValue<string>("CORS:" + idx);
                }



                services.AddCors(options =>
                {
                    options.AddPolicy("RuntimeCORS", builder =>
                    {

                        builder
                            .WithOrigins(allOrigins.ToArray())
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();

                    });
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to add Runtime CORS");
                Console.WriteLine(ex);
            }

            return services;
        }

        public static IServiceCollection WithCognitoJWT(this IServiceCollection services, IConfiguration Configuration)
        {
            try
            {
                var aud = Configuration.GetValue<string>("Authorization:Audience");
                var iss = Configuration.GetValue<string>("Authorization:Issuer");
                var auth = Configuration.GetValue<string>("Authorization:Authority");
                var grpFld = Configuration.GetValue<string>("Authorization:GroupField");
                var idFld = Configuration.GetValue<string>("Authorization:IdField");

                var userPoolId = Configuration.GetValue<string>("Cognito:PoolID");
                AmazonCognitoIdentityProviderClient client = new AmazonCognitoIdentityProviderClient();


                String idField = idFld == null ? "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" : idFld;

                //TODO: Dont pull this from the JWT ...
                String groupsField = grpFld == null ? "cognito:groups" : grpFld;

                var keyCache = new MemoryCache(new MemoryCacheOptions());

                services
                    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.Authority = auth;
                        options.ClaimsIssuer = iss;
                        options.Audience = aud;
                        options.RequireHttpsMetadata = false;


                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                        //from https://stackoverflow.com/questions/53244446/how-to-validate-aws-cognito-jwt-in-net-core-web-api-using-addjwtbearer
                        IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) =>
                            {
                            // get JsonWebKeySet from AWS
                            var json = keyCache.Get<string>(parameters.ValidIssuer);
                                if (json == null)
                                {
                                    json = new WebClient().DownloadString(parameters.ValidIssuer + "/.well-known/jwks.json");

                                    keyCache.Set<string>(parameters.ValidIssuer, json, DateTime.Now.AddMinutes(60));
                                }

                            // serialize the result
                            var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(json).Keys;

                            // cast the result to be the type expected by IssuerSigningKeyResolver
                            return (IEnumerable<SecurityKey>)keys;
                            },

                            ValidateIssuerSigningKey = true,
                            ValidIssuer = iss,
                            ValidateIssuer = true,
                            ValidateLifetime = true,
                            ValidAudience = aud,
                            ValidateAudience = false,
                        };
                        options.Events = new JwtBearerEvents
                        {
                            OnTokenValidated = async ctx =>
                            {
                            //TODO: Pull the groups from cognito for the particular user. This will ensure if the user's groups are changed they wont need to sign/out and back in
                            //Obtain the ID from the provided claims



                            string oid = ctx.Principal.FindFirstValue(idField);
                                var token = ctx.HttpContext.Request.Headers.Where(a => a.Key == "Authorization").Select(a => a.Value.FirstOrDefault()).FirstOrDefault();






                                List<string> allGroups = ctx.Principal.FindAll(groupsField).Select(a => a.Value).ToList();

                                try
                                {
                                //Basically we grab the actual groups
                                var groupRequest = new AdminListGroupsForUserRequest();
                                    groupRequest.Username = oid;
                                    groupRequest.UserPoolId = userPoolId;
                                    var groupResponse = await client.AdminListGroupsForUserAsync(groupRequest);

                                    allGroups = (from grp in groupResponse.Groups select grp.GroupName).ToList();


                                }
                                catch
                                {
                                //TODO
                            }

                                var claims = from grp in allGroups
                                             select new Claim("Group", grp);


                                var appIdentity = new ClaimsIdentity(ctx.Principal.Identity, claims);


                                ctx.Principal.AddIdentity(appIdentity);



                                token = token == null ? "" : token;

                                var tokenSansBearer = token.Replace("Bearer ", "");

                                var aclIdentity = new ClaimsIdentity(appIdentity, new List<Claim> { new Claim("id", oid), new Claim("token", tokenSansBearer) });

                                ctx.Principal.AddIdentity(aclIdentity);

                                if (forceSignout)
                                {
                                    ctx.Fail("YOU SHAL NOT PASS");
                                }


                            },
                        };

                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to add cognito. Can we read the config?");
                Console.WriteLine(ex);
            }

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
        public void ConfigureCognito(IServiceCollection services)
        {
            try
            {

                var openIDResponseType = Configuration["Authentication:Cognito:ResponseType"];
                var openIDMetadataAddress = Configuration["Authentication:Cognito:MetadataAddress"];
                var openIDClientId = Configuration["Authentication:Cognito:ClientId"];
                var openIDClientSecret = Configuration["Authentication:Cognito:ClientSecret"];
                var userPoolId = Configuration.GetValue<string>("Authentication:Cognito:PoolID");

                if (openIDClientId == null)
                {
                    Console.WriteLine("ClientId is missing.");
                }

                if (openIDClientSecret == null)
                {
                    Console.WriteLine("ClientSecret is missing.");
                }

                if (openIDMetadataAddress == null)
                {
                    Console.WriteLine("MetadataAddress is missing.");
                }

                if (openIDResponseType == null)
                {
                    Console.WriteLine("ResponseType is missing.");
                }

                if (userPoolId == null)
                {
                    Console.WriteLine("PoolID is missing.");
                }

                if (openIDClientId == null || openIDClientSecret == null || openIDMetadataAddress == null || openIDResponseType == null || userPoolId == null)
                {
                    Console.WriteLine("Unable to add Cognito. Ensure the correct parameters are set.");
                    return;
                }

                AmazonCognitoIdentityProviderClient client = new AmazonCognitoIdentityProviderClient();


                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
               {
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = new TimeSpan(0, 30, 0);
                    options.Cookie.SameSite = SameSiteMode.None;
                   options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Events = new CookieAuthenticationEvents()
                    {
                        OnSignedIn = context =>
                        {

                            return Task.CompletedTask;
                        },
                        OnValidatePrincipal = async context =>
                        {
                            try
                            {
                                string oid = context.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

                            //Basically we grab the actual groups
                            //TODO: Cache here
                            var groupRequest = new AdminListGroupsForUserRequest();
                                groupRequest.Username = oid;
                                groupRequest.UserPoolId = userPoolId;
                                var groupResponse = await client.AdminListGroupsForUserAsync(groupRequest);

                                var allGroups = (from grp in groupResponse.Groups select grp.GroupName).ToList();

                                var claims = from grp in allGroups
                                             select new Claim("Group", grp);

                                var usr = await client.AdminGetUserAsync(new AdminGetUserRequest { Username = oid, UserPoolId = userPoolId });

                                if (!usr.Enabled)
                                {
                                //await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                                //await context.HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

                                context.RejectPrincipal();


                                    return;
                                }



                                var appIdentity = new ClaimsIdentity(context.Principal.Identity, claims);


                                context.Principal.AddIdentity(appIdentity);


                            }
                            catch
                            {
                            //TODO
                        }

                        },
                        OnRedirectToAccessDenied = context =>
                        {
                            context.Response.StatusCode = 403;
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddOpenIdConnect(options =>
                {
                    options.ResponseType = openIDResponseType;
                    options.MetadataAddress = openIDMetadataAddress;
                    options.ClientId = openIDClientId;
                    options.ClientSecret = openIDClientSecret;
                    options.CallbackPath = "/api/signin-oidc";

                    options.Events = new OpenIdConnectEvents()
                    {
                        OnRedirectToIdentityProvider = context =>
                        {
                            if ((context.Request.ContentType != null && context.Request.ContentType.ToLower().StartsWith("application/json")) || (context.Request.GetTypedHeaders().Accept != null && context.Request.GetTypedHeaders().Accept.Contains(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/json"))))
                            {
                                context.Response.Clear();
                                context.Response.StatusCode = 401;
                                context.HandleResponse();
                                return Task.FromResult(0);
                            }

                            return Task.FromResult(0);
                        },
                        OnRedirectToIdentityProviderForSignOut = context =>
                        {
                            var logoutUri = Configuration["Authentication:Cognito:LogOutAddress"];
                            if (logoutUri != null)
                                logoutUri += "?response_type=code&client_id=" + openIDClientId + string.Format("&logout_uri={0}://{1}/api/Account/signout-callback-oidc", context.Request.Scheme, context.Request.Host);
                            context.Response.Redirect(logoutUri);
                            context.HandleResponse();

                            return Task.CompletedTask;
                        },
                        OnRemoteFailure = context =>
                        {
                        //This is kinda hacky
                        if (context.Failure.Message == "Correlation failed.")
                            {
                                context.Response.Redirect("/api/Account/SignIn");
                                context.HandleResponse();
                            }
                            else if (context.Failure.Message == "Unauthorized.")
                            {
                                context.Response.Redirect("/api/Account/SignOut");
                                context.HandleResponse();
                            }
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            context.Fail("Unauthorized.");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnAccessDenied = context =>
                        {
                            context.Response.Redirect("/api/Account/SignOut");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        },
                        OnRemoteSignOut = context =>
                        {
                            context.Fail("Unauthorized.");
                            return Task.CompletedTask;
                        },

                    };
                });

                services.WithCognitoJWT(Configuration);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to add cognito. Are we able to ge tthe config?");
                Console.WriteLine(ex);
            }
        }

        public void ConfigureSwager(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
               .WithDependencyInjection(Configuration);

            services.WithDevelopmentCORS();
            services.WithRuntimeCORS(Configuration);


            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardLimit = 2;
                options.KnownProxies.Clear();
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
            });

            try
            {
                ConfigureCognito(services);
            }
            catch
            {
                Console.WriteLine("Unable to configure cognito. Continuing without.");
            }


            services
                .AddControllers()
                .AddNewtonsoftJson(o =>
                {
                    o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

        }

        public void addSwagger(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
        }


        public void addStaticHosting(IApplicationBuilder app, IWebHostEnvironment env)
        {
            try
            {
                PhysicalFileProvider fileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), @"static"));
                DefaultFilesOptions defoptions = new DefaultFilesOptions();

                defoptions.DefaultFileNames.Clear();
                defoptions.FileProvider = fileProvider;
                defoptions.DefaultFileNames.Add("index.html");
                app.UseDefaultFiles(defoptions);

                app.UseStaticFiles();
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = fileProvider,
                    RequestPath = new PathString(""),
                    OnPrepareResponse = ctx =>
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age=60");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not add static file hosting.");
                Console.WriteLine(ex);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors("DevelopmentCORS");
            }

            try
            {
                app.UseCors("RuntimeCORS");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Unable to add runtime CORS");
                Console.WriteLine(ex);
            }

            app.UseHttpsRedirection();
            app.UseForwardedHeaders();

            try
            {
                app.UseAuthentication();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Unable to add authentication");
                Console.WriteLine(ex);
            }

            app.UseRouting();

            try
            {
                app.UseAuthorization();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Unable to add authorization");
                Console.WriteLine(ex);
            }

            addStaticHosting(app, env);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
