
using com.timmons.Stitch.Shared;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using API.Middleware.Helpers;

using Model = API.Middleware.Model;

namespace API.Helpers
{
    /*
     * We implement this as a lookupable for a Tuple String/Int because we need to look up by a config key and a value within that config
     * We also implement as the ienumerable to get a choice by a "path" for multi level choices
     */
    public class GenericConfigHelper : Lookupable<object, Tuple<string, int>>, Lookupable<object, Tuple<string, IEnumerable<int>>>
    {
        private static readonly IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        IAppConfigMapper mapper;
        IConnection con;

        public GenericConfigHelper(IAppConfigMapper configuration, IConnection con)
        {
            this.con = con;
            this.mapper = configuration;
        }

        public async IAsyncEnumerable<object> Get(IEnumerable<int> choiceIDs, string lookupKey)
        {
            foreach (var id in choiceIDs)
            {
                var cacheKey = new Tuple<string, int>(lookupKey, id);

                var cachedCopy = cache.Get<Model.DTOs.ChoiceDTO>(cacheKey);

                if (cachedCopy != null)
                    yield return cachedCopy;
                else
                {

                    var newDTO = new Model.DTOs.ChoiceDTO
                    {
                        id = id,
                        name = "Unknown"
                    };


                    string sql = @"SELECT config->>@key FROM stitch.appconfig WHERE appname = @app ORDER BY id DESC LIMIT 1";
                    var val = await con.Wrap().GetConnection().QuerySingleAsync<String>(sql, new { key = id.ToString(), app = lookupKey });

                    if (val != null)
                    {
                        newDTO.name = val;
                    }

                    cache.Set<Model.DTOs.ChoiceDTO>(cacheKey, newDTO, DateTime.Now.AddMinutes(5));

                    yield return newDTO;
                }

            }

            yield break;
        }



        public async IAsyncEnumerable<object> GetAll(string lookupKey)
        {

            string sql = @"SELECT config FROM stitch.appconfig WHERE appname = @app ORDER BY id DESC LIMIT 1";
            JObject jval = null;

            try
            {
                jval = await con.Wrap().GetConnection().QuerySingleAsync<JObject>(sql, new { app = lookupKey });
            }
            catch { } //Oopsie

            if (jval == null)
                yield break;

            foreach(var pair in jval)
            {
                var key = int.Parse(pair.Key);
                var val = pair.Value.Value<string>();

                yield return new Model.DTOs.ChoiceDTO
                {
                    id = key,
                    name = val
                };
            }

            yield break;
        }

                
        async IAsyncEnumerable<object> Lookupable<object, Tuple<string, int>>.Get(IEnumerable<Tuple<string, int>> lookup)
        {
            foreach (var luid in lookup)
            {
                var lookupKey = luid.Item1;
                var id = luid.Item2;

                var cacheKey = new Tuple<string, int>(lookupKey, id);

                var cachedCopy = cache.Get<Model.DTOs.ChoiceDTO>(cacheKey);

                if (cachedCopy == null)
                {
                    //Value isnt cached

                    var newDTO = new Model.DTOs.ChoiceDTO
                    {
                        id = id,
                        name = "Unknown"
                    };


                    string sql = @"SELECT config->>@key FROM stitch.appconfig WHERE appname = @app ORDER BY id DESC LIMIT 1";
                    var val = await con.Wrap().GetConnection().QuerySingleAsync<string>(sql, new { key = id.ToString(), app = lookupKey });

                    if (val != null)
                    {
                        newDTO.name = val;
                    }

                    cache.Set<Model.DTOs.ChoiceDTO>(cacheKey, newDTO, DateTime.Now.AddMinutes(5));
                    cachedCopy = newDTO;
                }
                    
                
                
                yield return cachedCopy;
                

            }

            yield break;
        }

        async IAsyncEnumerable<object> Lookupable<object, Tuple<string, IEnumerable<int>>>.Get(IEnumerable<Tuple<string, IEnumerable<int>>> lookup)
        {
            foreach (var luid in lookup)
            {
                var lookupKey = luid.Item1;
                var idPath = luid.Item2;

                var id = idPath.LastOrDefault();

                var cacheKey = new Tuple<string, IEnumerable<int>>(lookupKey, idPath);

                var cachedCopy = cache.Get<Model.DTOs.ChoiceDTO>(cacheKey);

                if (cachedCopy != null)
                    yield return cachedCopy;
                


                //Cache wasnt set
                var newDTO = new Model.DTOs.ChoiceDTO
                {
                    id = id,
                    name = "Unknown"
                };


                string sql = @"SELECT config#>>@key FROM stitch.appconfig WHERE appname = @app ORDER BY id DESC LIMIT 1";
                var val = await con.Wrap().GetConnection().QuerySingleAsync<String>(sql, new { key = (from i in idPath select i.ToString()).ToList(), app = lookupKey });

                if (val != null)
                {
                    newDTO.name = val;
                }

                cache.Set<Model.DTOs.ChoiceDTO>(cacheKey, newDTO, DateTime.Now.AddMinutes(5));

                yield return newDTO;
                
            }

            yield break;
        }

        async Task<object> Lookupable<object, Tuple<string, int>>.GetSingle(Tuple<string, int> lookup)
        {
            
            var lookupKey = lookup.Item1;
            var id = lookup.Item2;

            var cacheKey = new Tuple<string, int>(lookupKey, id);

            var cachedCopy = cache.Get<Model.DTOs.ChoiceDTO>(cacheKey);

            if (cachedCopy != null)
                return cachedCopy;
            

            //The value wasnt cached, get a new version
            var newDTO = new Model.DTOs.ChoiceDTO
            {
                id = id,
                name = "Unknown"
            };


            string sql = @"SELECT config->>@key FROM stitch.appconfig WHERE appname = @app ORDER BY id DESC LIMIT 1";
            var val = await con.Wrap().GetConnection().QuerySingleAsync<string>(sql, new { key = id.ToString(), app = lookupKey });

            if (val != null)
            {
                newDTO.name = val;
            }

            cache.Set<Model.DTOs.ChoiceDTO>(cacheKey, newDTO, DateTime.Now.AddMinutes(5));

            return newDTO;
            
        }

        async Task<object> Lookupable<object, Tuple<string, IEnumerable<int>>>.GetSingle(Tuple<string, IEnumerable<int>> lookup)
        {
            
            var lookupKey = lookup.Item1;
            var idPath = lookup.Item2;

            var id = idPath.LastOrDefault();

            var cacheKey = new Tuple<string, IEnumerable<int>>(lookupKey, idPath);

            var cachedCopy = cache.Get<Model.DTOs.ChoiceDTO>(cacheKey);

            if (cachedCopy != null)
                return cachedCopy;


            //The value wasnt cached, get a new version
            var newDTO = new Model.DTOs.ChoiceDTO
                {
                    id = id,
                    name = "Unknown"
                };


            string sql = @"SELECT config#>>@key FROM stitch.appconfig WHERE appname = @app ORDER BY id DESC LIMIT 1";
            var val = await con.Wrap().GetConnection().QuerySingleAsync<String>(sql, new { key = (from i in idPath select i.ToString()).ToList(), app = lookupKey });

            if (val != null)
            {
                newDTO.name = val;
            }

            cache.Set<Model.DTOs.ChoiceDTO>(cacheKey, newDTO, DateTime.Now.AddMinutes(5));

            return newDTO;
                

        }
    }
}
