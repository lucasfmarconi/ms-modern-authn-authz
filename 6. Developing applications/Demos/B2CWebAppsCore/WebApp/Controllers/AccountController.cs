using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace WebApp.Controllers
{
    public class AccountController : Controller
    {
        public AccountController(IConfiguration conf)
        {
            Configuration = conf;
        }
        public IConfiguration Configuration { get; set; }
        [AllowAnonymous]
        public IActionResult SignIn()
        {
            var redirectUrl = Url.Action(nameof(HomeController.Index), "Home");
            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl },
                AzureADB2CDefaults.AuthenticationScheme);
        }
        public IActionResult SignOut()
        {
            AzureADB2COptions options = new AzureADB2COptions();
            Configuration.Bind("AzureADB2C", options);
            //var callbackUrl = Url.Action(nameof(SignedOut), "Account", values: null, protocol: Request.Scheme);
            return SignOut(
                new AuthenticationProperties { RedirectUri = options.SignedOutCallbackPath },
                AzureADB2CDefaults.CookieScheme,
                AzureADB2CDefaults.OpenIdScheme);
        }
        [AllowAnonymous]
        public IActionResult SignedOut()
        {
            return View();
        }
    }
}