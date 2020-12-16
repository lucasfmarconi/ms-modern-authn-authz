using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace B2CDemo.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            foreach (var c in ClaimsPrincipal.Current.Claims)
            {

            }
            return View();
        }

        public async Task<ActionResult> About()
        {
            var userId = ClaimsPrincipal.Current.Claims.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
            var http = new HttpClient();
            var accts = await Startup.MSALApp.GetAccountsAsync();
            var acct = accts.FirstOrDefault(
                a => a.HomeAccountId.ObjectId.StartsWith(userId));

            var tokens = await Startup.MSALApp.AcquireTokenSilent(
                new string[] { "https://mrochonb2cprod.onmicrosoft.com/demoapi/read" },
                acct
                ).ExecuteAsync();

            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                tokens.AccessToken);
            var resp = await http.GetStringAsync("https://localhost:44349/api/values");

            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}