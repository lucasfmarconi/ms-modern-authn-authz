using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WebAppDemo.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public async Task<ActionResult> Contact()
        {
            ViewBag.Message = "Your contact page.";

            var userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            var client = ConfidentialClientApplicationBuilder
                .Create(Startup.clientId)
                .WithClientSecret(Startup.clientKey)
                .WithAuthority(Startup.authority)
                .WithRedirectUri($"https://{Request.Url.Host}:{Request.Url.Port}/signin-oidc")
                .Build()
                .WithSessionCache(HttpContext, userId);
            var acct = (await client.GetAccountsAsync()).First();
            var tokens = await(client.AcquireTokenSilent(Startup.scopes, acct).ExecuteAsync());
            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            var results = await http.GetStringAsync("https://localhost:44358/api/values");
            return View();
        }
    }
}