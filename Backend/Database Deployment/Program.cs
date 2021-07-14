using DbUp;
using DbUp.Engine;
using DbUp.Postgresql;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DatabaseDeployment
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var ssmPath =
                args.Take(1).FirstOrDefault();


            var builder = new ConfigurationBuilder();

            builder.AddSystemsManager(ssmPath);

            var conf = builder.Build();

            var connectionString = conf.GetValue<string>("connectionString");



            var clientKey =
                "Utah";

            var capabilities = args.Skip(1);

            if(capabilities.Contains("All"))
            {
                capabilities = new System.Collections.Generic.List<string>
                {
                    "Stitch",
                    "Projects",
                    "Users",
                    "Properties",
                    "Clients",
                    "Instruments",
                    "Deeds",
                    "SpecialUsePermits",
                    "SurfaceUseAgreements",
                    "ConservationEasements",
                    "OtherAgreements",
                    "Events",
                    "Amendments"
                };
            }


            
            logWhite("Beginning main PAM Core deployment");
            

#if DEBUG
            if (connectionString == null)
            {
                connectionString = "Server=localhost;Port=5432;Database=PAM;Username=PAM_LOCAL;Password=PAM_LOCAL;";
                logYellow("Enter connection string or none for default (" + connectionString + ")");
                var o = Console.ReadLine();
                if (o.Length > 0)
                    connectionString = o;
            }

            if (clientKey == null)
            {
                clientKey = "Utah";
                logYellow("Enter clientKey or none for default (" + clientKey + ")");
                var o = Console.ReadLine();
                if (o.Length > 0)
                    clientKey = o;
            }

#endif

            EnsureDatabase.For.PostgresqlDatabase(connectionString);

            var result = deploy(DeployFrom.Embedded, "Core", connectionString, Assembly.GetExecutingAssembly());

            if (!result.Successful)
            {
                logRed("PAM deployment failed!");
                logRed(result.Error);
                
#if DEBUG
                Console.ReadLine();
#endif
                return -1;
            }

            
            logGreen("PAM core deployment success!");


            int succeededCapabilities = 0, failedCapabilities = 0, totalCapabilities = 0, unknownCapabilities = 0;


            if(capabilities.Any())
            {
                totalCapabilities = capabilities.Count();
                foreach(String capability in capabilities)
                {
                    //TODO split off the first character so that we can dow the components as well
                    //eg +FAM +FireMAP -FAM -FireMap
                    DatabaseUpgradeResult componentResult = null;

                    switch (capability.ToLower())
                    {
                        case "stitch":
                            logYellow("Deploying feature Stitch");
                            componentResult = deploy(DeployFrom.Folder, "Stitch", connectionString, new FolderUpgrade { clientKey = clientKey, featurePath = "Capabilities/Stitch" });
                            break;
                        case "projects":
                            logYellow("Deploying feature projects");
                            componentResult = deploy(DeployFrom.Folder, "Projects", connectionString, new FolderUpgrade { clientKey = clientKey, featurePath = "Capabilities/Projects" });
                            break;
                        case "properties":
                            logYellow("Deploying feature properties");
                            componentResult = deploy(DeployFrom.Folder, "Properties", connectionString, new FolderUpgrade { clientKey = clientKey, featurePath = "Capabilities/Properties" });
                            break;
                        case "users":
                            logYellow("Deploying feature users");
                            componentResult = deploy(DeployFrom.Folder, "Users", connectionString, new FolderUpgrade { clientKey = clientKey, featurePath = "Capabilities/Users" });
                            break;
                        case "clients":
                            logYellow("Deploying feature clients");
                            componentResult = deploy(DeployFrom.Folder, "Clients", connectionString, new FolderUpgrade { clientKey = clientKey, featurePath = "Capabilities/Clients" });
                            break;
                        case "instruments":
                            logYellow("Deploying feature instruments");
                            componentResult = deploy(DeployFrom.Folder, "Instruments", connectionString, new FolderUpgrade { clientKey = clientKey, featurePath = "Capabilities/Instruments" });
                            break;
                        case "deeds":
                            logYellow("Deploying feature deeds");
                            componentResult = deploy(DeployFrom.Folder, "Deeds", connectionString, new FolderUpgrade { clientKey = clientKey, featurePath = "Capabilities/Deeds" });
                            break;
                        case "specialusepermits":
                            logYellow("Deploying feature special use permits");
                            componentResult = deploy(DeployFrom.Folder, "SpecialUsePermits", connectionString, new FolderUpgrade { clientKey = clientKey, featurePath = "Capabilities/SpecialUsePermits" });
                            break;
                        case "surfaceuseagreements":
                            logYellow("Deploying feature surface use agreements");
                            componentResult = deploy(DeployFrom.Folder, "SurfaceUseAgreements", connectionString, new FolderUpgrade { clientKey = clientKey, featurePath = "Capabilities/SurfaceUseAgreements" });
                            break;
                        case "conservationeasements":
                            logYellow("Deploying feature conservation easements");
                            componentResult = deploy(DeployFrom.Folder, "ConservationEasements", connectionString, new FolderUpgrade { clientKey = clientKey, featurePath = "Capabilities/ConservationEasements" });
                            break;
                        case "otheragreements":
                            logYellow("Deploying feature other agreements");
                            componentResult = deploy(DeployFrom.Folder, "OtherAgreements", connectionString, new FolderUpgrade { clientKey = clientKey, featurePath = "Capabilities/OtherAgreements" });
                            break;
                        case "events":
                            logYellow("Deploying feature events");
                            componentResult = deploy(DeployFrom.Folder, "Events", connectionString, new FolderUpgrade { clientKey = clientKey, featurePath = "Capabilities/Events" });
                            break;
                        case "amendments":
                            logYellow("Deploying feature amendments");
                            componentResult = deploy(DeployFrom.Folder, "Amendments", connectionString, new FolderUpgrade { clientKey = clientKey, featurePath = "Capabilities/Amendments" });
                            break;
                        default:
                            unknownCapabilities++;
                            logRed("Unknown capability: " + capability);
                            break;

                    }

                    if(componentResult != null)
                    {
                        if(componentResult.Successful)
                        {
                            succeededCapabilities++;
                        }
                        else
                        {
                            logRed("Component failed to install.");
                            failedCapabilities++;
                        }
                    }
                }
            }

            if(succeededCapabilities != totalCapabilities)
            {
                logRed("Failed to deploy some capabilities. See the prior log");
#if DEBUG
                //Console.ReadLine();
#endif
                return -1;
            }
#if DEBUG
            //Console.ReadLine();
#endif
            return 0;
        }

        static DatabaseUpgradeResult deploy(DeployFrom from, String componentName, String connString, Object arg)
        {
            var upgrader =
                DeployChanges.To
                    .PostgresqlDatabase(connString);

            switch(from)
            {
                case DeployFrom.Embedded:
                    upgrader = upgrader.WithScriptsEmbeddedInAssembly((Assembly)arg);
                    break;
                case DeployFrom.Folder:
                    var folderUntgrad = (FolderUpgrade)arg;
                    upgrader = upgrader.WithScriptsFromFileSystem(Path.Combine(Environment.CurrentDirectory, folderUntgrad.featurePath, folderUntgrad.clientKey));
                    break;
            }

            upgrader =
                    upgrader.LogToConsole();
            
            var app = upgrader.Build();



            var result = app.PerformUpgrade();

            if (!result.Successful)
            {
                logRed("Deployment failed for component " + componentName);
                logRed(result.Error);

#if DEBUG
                Console.ReadLine();
#endif
                return result;
            }

            return result;
        }

        static void logRed(Object val)
        {
            log(ConsoleColor.Red, val);
        }

        static void logGreen(Object val)
        {
            log(ConsoleColor.Green, val);
        }

        static void logWhite(Object val)
        {
            log(ConsoleColor.White, val);
        }

        static void logYellow(Object val)
        {
            log(ConsoleColor.Yellow, val);
        }


        static void log(ConsoleColor color, Object val)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(val);
            Console.ResetColor();
        }
    }

    internal class FolderUpgrade
    {
        public string featurePath { get; set; }
        public string clientKey { get; set; }
    }

    internal enum DeployFrom
    {
        Folder,
        Embedded
    }
}
