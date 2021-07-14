using com.timmons.Stitch.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("Account/[action]")]
    [Route("api/Account/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IConnection con;
        private readonly IACLMapper aclMapper;

        public AccountController(IConfiguration conf, IConnection con, IACLMapper mapper)
        {
            this.configuration = conf;
            this.con = con;
            this.aclMapper = mapper;
        }

        [ActionName("SignOut")]
        public async Task<ActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            
            return new EmptyResult();
        }

        [ActionName("signout-callback-oidc")]
        public ActionResult SignOutCallback()
        {
            return new RedirectResult("/#/SignedOut");
        }


        [ActionName("SignIn")]
        [Authorize]
        public async Task<ActionResult> SignIn(string returnTo)
        {
            var ur = HttpContext.User;

            if (ur.Identity.IsAuthenticated)
            {
                if (returnTo != null)
                {
                    if (!returnTo.StartsWith("/") && !returnTo.StartsWith("http"))
                        returnTo = "/" + returnTo;

                    return new RedirectResult(returnTo, false, true);
                }

                return new RedirectResult("/", false, true);
            }
            else
                await HttpContext.ChallengeAsync();

            return new RedirectResult("/", false, true);
        }

        [ActionName("IsSignedIn")]
        public bool IsSignIn()
        {
            var ur = HttpContext.User;

            if (ur.Identity.IsAuthenticated)
                return true;
            else
                return false;
        }

        [ActionName("Profile")]
        public Profile GetProfile()
        {
            var usr = HttpContext.User;

            if (!usr.Identity.IsAuthenticated)
                return null;



            var emailClaim = usr.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
            var firstNameClaim = usr.FindFirst("custom:firstName");
            var lastNameClaim = usr.FindFirst("custom:lastName");

            return new Profile
            {
                email = emailClaim != null ? emailClaim.Value : "",
                firstName = firstNameClaim != null ? firstNameClaim.Value : "",
                lastName = lastNameClaim != null ? lastNameClaim.Value : ""
            };
        }

        [ActionName("GetACLs")]
        [Authorize]
        public List<string> GetACLs()
        {
            return this.aclMapper.Get(HttpContext.User);
        }
    }

    public class Profile
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }

    }
}
