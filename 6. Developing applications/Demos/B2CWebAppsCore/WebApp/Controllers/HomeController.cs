using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(IConfiguration conf, IConfidentialClientApplication auth)
        {
            Configuration = conf;
            _auth = auth;
        }
        public IConfiguration Configuration { get; set; }
        private IConfidentialClientApplication _auth;
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Privacy()
        {
            var http = new HttpClient();

            var accts = await _auth.GetAccountsAsync();

            var scopes = new string[] { "https://mrochonb2cprod.onmicrosoft.com/webapi/read_policies" };
            var acct = (await _auth.GetAccountsAsync()).First();
            var tokens = await _auth.AcquireTokenSilent(scopes, acct).ExecuteAsync();

            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            var result = await http.GetStringAsync("https://localhost:44317/api/values");
            ViewBag.Result = result;
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
