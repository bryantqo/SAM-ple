using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Extensions.Configuration
{
    public static class StitchConfigurationExtensions
    {
        public static IConfigurationBuilder AddStitchConfig(this IConfigurationBuilder builder)
        {
            var b = builder.Build();


            var ConStr = "";
            try
            {
                ConStr = b.GetValue<string>("connectionString");

                //DatabaseDeployment.Program._Main(new string[] { ConStr, "Stitch" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to deploy Stitch");
            }

            return builder.Add(new StitchConfigSource(ConStr));
        }
    }

    public class ConfigItem
    {
        public int id { get; set; }
        public string appname { get; set; }
        public JToken config { get; set; }
    }

    public class StitchConfigSource : IConfigurationSource
    {
        private readonly string constr;

        public StitchConfigSource(string constr)
        {
            this.constr = constr;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new StitchProvider(constr);
        }
    }

    public class StitchProvider : ConfigurationProvider
    {
        private Dictionary<string, JToken> cfg = new Dictionary<string, JToken>();

        private readonly string constr;

        public StitchProvider(string constr)
        {
            this.constr = constr;
        }

        public override void Load()
        {
            try
            {
                Console.WriteLine("Attempting to load configuration from database");

                using (var con = new NpgsqlConnection(constr))
                {
                    var sql = @"
                        SELECT 
                            id,
                            appname,
                            config
                        FROM stitch.appconfig
                        ORDER BY id
                        ";

                    var itms = con.Query<ConfigItem>(sql).ToList();

                    foreach (var itm in itms)
                    {
                        cfg[itm.appname] = itm.config;
                    }

                    Data = new Dictionary<string, string>();
                    
                    long itemCount = 0;
                    
                    foreach(var key in cfg.Keys)
                    {
                        var val = cfg[key];

                        var flat = flatten(key, val);
                        foreach (var p in flat)
                        {
                            Data[p.Item1] = p.Item2;
                            itemCount++;
                        }
                    }

                    Console.WriteLine("Added " + itemCount + " config items");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to retreive config from stitch");
                Console.WriteLine(ex.ToString());
            }
        }

        static List<Tuple<string,string>> flatten(string rootPath, JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object: return (from k in token.Children() select flatten(rootPath + ":" + k.Path.Split(".").Last(), k.First())).SelectMany(a => a).ToList();
                case JTokenType.Array:
                    {
                        var ind = 0;
                        List<List<Tuple<string, string>>> flats = new List<List<Tuple<string, string>>>();
                        foreach (var idx in token as JArray)
                        {
                            var flat = flatten(rootPath + ":" + ind, idx);
                            flats.Add(flat);
                            ind++;
                        }
                        return flats.SelectMany(a => a).ToList();
                    }
                default: case JTokenType.String: return new List<Tuple<string, string>> { new Tuple<string, string>(rootPath, token.ToString()) };
            }
        }

        
    }

}
