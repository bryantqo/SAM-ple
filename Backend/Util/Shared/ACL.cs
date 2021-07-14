using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace com.timmons.Stitch.Shared
{
    public class GroupACLCache
    {
        private readonly IMemoryCache cache;

        public GroupACLCache()
        {
            cache = new MemoryCache(new MemoryCacheOptions());
        }

        public List<String> Get(String groupName)
        {
            return cache.Get<List<String>>(groupName);
        }

        public void Put(String groupName, List<String> acls)
        {
            cache.Set<List<String>>(groupName, acls, DateTime.Now.AddMinutes(1)); //Only cache for 1 minute tops
        }
    }    
}
