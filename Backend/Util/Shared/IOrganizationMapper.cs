using Dapper;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace com.timmons.Stitch.Shared
{
    public class Organization
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class UserOrgCache
    {
        private readonly IMemoryCache cache;

        public UserOrgCache()
        {
            cache = new MemoryCache(new MemoryCacheOptions());
        }

        public List<Organization> Get(String userId)
        {
            return cache.Get<List<Organization>>(userId);
        }

        public void Put(String userid, List<Organization> orgs)
        {
            cache.Set<List<Organization>>(userid, orgs, DateTime.Now.AddMinutes(1)); //Only cache for 1 minute tops
        }
    }

    public interface IOrganizationMapper
    {
        List<Organization> Get(ClaimsPrincipal user);
    }

    public class PAMOrganizationMapper : IOrganizationMapper
    {

        private static UserOrgCache orgCache = new UserOrgCache();

        private IConnection connection;

        public PAMOrganizationMapper(IConnection con)
        {
            connection = con;
        }


        public List<Organization> Get(ClaimsPrincipal user)
        {
            //This function will take a claims principle and map all of the ACLs from the PAM database based on the user's groups
            var iid = (from i in user.Claims where i.Type == "id" select i.Value).FirstOrDefault();
            

            //We dont have an ID for this user ...
            if (iid == null)
                return new List<Organization> { };

            var gid = Guid.Parse(iid);

            //Check our cache, this keep sus from hitting the database for every request
            var userOrgs = orgCache.Get("user:" + iid);

            if (userOrgs == null)
            {
                userOrgs = new List<Organization>();
                var db = connection;

                
                try
                {
                    var orgs = db.Wrap().GetConnection().Query<Organization>(@"
SELECT
org.id,
org.name
FROM cognitive_1_0_0.link_organization_members org_members
LEFT JOIN cognitive_2_0_0.objects_organization org
on org_members.organization_id = org.id
        WHERE org_members.member_id = @id

    ", new { id = gid });

                    userOrgs.AddRange(orgs);
                    orgCache.Put("user:" + iid, userOrgs);
                }
                catch (Exception ex)
                {
                }
                userOrgs = userOrgs.Distinct().ToList();
            }

            return userOrgs;
        }
    }
}
