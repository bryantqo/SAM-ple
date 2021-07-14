using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Handlers
{
    public class HasACLRequirement : IAuthorizationRequirement
    {
        public string[] acls { get; private set; }

        public HasACLRequirement(string[] acls)
        {
            this.acls = acls;
        }
    }

    public class HasACLAttribute : AuthorizeAttribute
    {
        const string POLICY_PREFIX = "HasACL";

        public HasACLAttribute(string[] acls) => this.acls = acls;

        // Get or set the Age property by manipulating the underlying Policy property
        public string[] acls
        {
            get
            {
                return Policy.Substring(POLICY_PREFIX.Length).Split(",");
            }
            set
            {
                Policy = $"{POLICY_PREFIX}{String.Join(",",value)}";
            }
        }
    }
}
