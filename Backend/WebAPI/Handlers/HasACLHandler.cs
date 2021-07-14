using com.timmons.Stitch.Shared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Handlers
{
    public class HasACLHandler : AuthorizationHandler<HasACLRequirement>
    {
        IACLMapper aclMapper;
        public HasACLHandler(IACLMapper mapper)
        {
            this.aclMapper = mapper;
        }

        protected override Task HandleRequirementAsync( AuthorizationHandlerContext context,  HasACLRequirement requirement)
        {
            // Save User object to access claims
            var user = context.User;

            var perms = this.aclMapper.Get(user);

            foreach(var acl in requirement.acls)
            {
                if(!perms.Contains(acl))
                    return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

    public class HasACLPolicyProvider : IAuthorizationPolicyProvider
    {
        const string POLICY_PREFIX = "HasACL";
        private DefaultAuthorizationPolicyProvider BackupPolicyProvider { get; }

        public HasACLPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            BackupPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        //TODO: I get how I know that this is the right policy, but like, how can we build this to do it automatically and thus independent of my shinanigans 
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
            Task.FromResult(new AuthorizationPolicyBuilder(OpenIdConnectDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build());

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync() =>
            Task.FromResult<AuthorizationPolicy>(null);

        // Policies are looked up by string name, so expect 'parameters' (like age)
        // to be embedded in the policy names. This is abstracted away from developers
        // by the more strongly-typed attributes derived from AuthorizeAttribute
        // (like [MinimumAgeAuthorize()] in this sample)
        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(POLICY_PREFIX, StringComparison.OrdinalIgnoreCase) )
            {
                var policy = new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme);
                //policy.AddRequirements(new MinimumAgeRequirement(age));
                policy.AddRequirements(new HasACLRequirement(policyName.Substring(POLICY_PREFIX.Length).Split(",")));
                return Task.FromResult(policy.Build());
            }

            return BackupPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
