using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var auth = new AuthenticationContext(
                "https://login.microsoftonline.com/ModernAuthn.onmicrosoft.com/");

            // Using Confidential Client flow
            var tokenResp = auth.AcquireTokenAsync(
                "https://graph.windows.net",
                new ClientCredential(
                    "1c635c96-739f-4a45-8020-0ee6ba133681",
                    "/VEB4SadZvIOJ/ETtFGQR1EN4bKOb2FMDmljAl7c0cs=")).Result;
            Console.WriteLine("Access token:{0}", tokenResp.AccessToken);

            // Using Authorization Code Grant flow
            tokenResp = auth.AcquireTokenAsync(
                resource: "https://graph.windows.net",
                clientId: "1c767fef-4c4f-406b-81fc-21b6cd7b9eab",
                redirectUri: new Uri("https://modernauthn.com/"),
                parameters: new PlatformParameters(PromptBehavior.Always)
                ).Result;
            Console.WriteLine("Access token:{0}", tokenResp.AccessToken);


            // Using the token to call AAD Graph API
            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                tokenResp.AccessToken);
            var me = http.GetStringAsync(
                "https://graph.windows.net/modernauthn.onmicrosoft.com/me?api-version=1.6").Result;
            Console.WriteLine(me);

            Console.ReadLine();
        }
    }
}
