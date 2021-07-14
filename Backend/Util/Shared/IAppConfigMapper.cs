using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System;

namespace com.timmons.Stitch.Shared
{
    public interface IAppConfigMapper
    {
        JObject Get(String app);
    }


    public class GroupAppConfigCache
    {
        private readonly IMemoryCache cache;

        public GroupAppConfigCache()
        {
            cache = new MemoryCache(new MemoryCacheOptions());
        }

        public JObject Get(String groupName)
        {
            return cache.Get<JObject>(groupName);
        }

        public void Put(String groupName, JObject AppConfigs)
        {
            cache.Set<JObject>(groupName, AppConfigs, DateTime.Now.AddMinutes(1)); //Only cache for 1 minute tops
        }
    }

    public class PAMAppConfigMapper : IAppConfigMapper
    {

        private static GroupAppConfigCache AppConfigCache = new GroupAppConfigCache();

        private IConnection connection;

        public PAMAppConfigMapper(IConnection con)
        {
            connection = con;
        }


        public JObject Get(String app)
        {
            


            //Check our cache, this keep sus from hitting the database for every request
            var appConfig = AppConfigCache.Get(app);

            if (appConfig == null)
            {
                appConfig = new JObject();
                var db = connection;

                //Dictionary<Guid, AppConfig> allEffectiveAppConfig = new Dictionary<Guid, AppConfig>();

                try
                {
                    appConfig = db.Wrap().GetConnection().QuerySingle<JObject>(@"
        SELECT config 
        FROM stitch.appConfig 
        WHERE LOWER(appName) = LOWER(@name)
        ORDER BY id DESC 
        LIMIT 1
    ", new { name = app });

                    AppConfigCache.Put(app, appConfig);
                }
                catch
                {
                    return null;
                }
            }

            return appConfig;
        }
    }
}
