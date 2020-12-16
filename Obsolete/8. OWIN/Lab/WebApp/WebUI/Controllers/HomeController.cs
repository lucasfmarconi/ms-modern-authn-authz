using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebUI.Models;

namespace WebUI.Controllers
{
    [WebUI.Utils.Authorize(Roles ="admin")]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> About()
        {
            var signedInUserID = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            var objId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            var authContext = new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext(Startup.Authority, new ADALTokenCache(signedInUserID));
            var authRes = await authContext.AcquireTokenSilentAsync(
                "https://modernauthn.onmicrosoft.com/WebAPI",
                new ClientCredential(Startup.clientId, Startup.appKey),
                new UserIdentifier(objId, UserIdentifierType.UniqueId));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authRes.AccessToken);
            var res = await client.GetStringAsync("http://localhost:44317/api/default");
            Response.Write(res);
            return null;
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}