using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace com.timmons.Stitch.Shared
{
    public interface IACLMapper
    {
        List<String> Get(ClaimsPrincipal user);
    }

    public class PAMACLMapper : IACLMapper
    {

        private static GroupACLCache aclCache = new GroupACLCache();

        private readonly IConnection connection;

        public PAMACLMapper(IConnection con)
        {
            connection = con;
        }


        public List<String> Get(ClaimsPrincipal user)
        {
            //This function will take a claims principle and map all of the ACLs from the PAM database based on the user's groups
            var iid = (from i in user.Claims where i.Type == "id" || i.Type == "cognito:username" select i.Value).FirstOrDefault();
            List<string> allGroups = user.FindAll("Group").Select(a => a.Value).Concat(user.FindAll("cognito:groups").Select(a => a.Value)).Distinct().ToList();
            allGroups.Add("DEFAULT");


            //We dont have an ID for this user ...
            if (iid == null)
                return new List<string> { };


            //Check our cache, this keep sus from hitting the database for every request
            var usersACLs = aclCache.Get("user:" + iid);

            if (usersACLs == null)
            {
                usersACLs = new List<String>();
                var db = connection;


                foreach (var grp in allGroups)
                {
                    if (aclCache.Get(grp) == null)
                    {
                        try
                        {
                            var acls = db.Wrap().GetConnection().Query<String>(@"
                SELECT
              jsonb_array_elements_text(acl.acl) as dat
             FROM pam.groupacl acl
                WHERE acl.groupkey = @name

            ", new { name = grp });

                            aclCache.Put(grp, acls.ToList());
                        }
                        catch (Exception ex)
                        {
                            //If we come across an exception the ACL list will be empty and that is ok
                        }
                    }

                    var gacl = aclCache.Get(grp);
                    if (gacl != null)
                    {
                        usersACLs.AddRange(gacl);
                    }
                }
                usersACLs = usersACLs.Distinct().ToList();
                aclCache.Put("user:" + iid, usersACLs);
            }

            return usersACLs;
        }
    }
}
