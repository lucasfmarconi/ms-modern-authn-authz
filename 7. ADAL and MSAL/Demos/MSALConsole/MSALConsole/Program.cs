using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;


// https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Client-Applications

namespace MSALConsole
{
    class Program
    {

        // Configuration data:
        // Public client gets tokens to call an API (confidential client)
        // Confidential client exchanges these tokens for other resources or token types

        // Registered app at: apps.dev.microsoft.com or your B2C tenant,
        //static string[] _scopes = new string[] { "user.read" };

        //static string _publicClientId = "26db6f21-ae94-49fd-86a1-49f873aa0fb4";
        //static string _publicClientRedirectURI = "msal26db6f21-ae94-49fd-86a1-49f873aa0fb4://auth";

        //static string _confidentialClientId = "52a3749f-4a89-456c-b762-e2bdb0216ecc";
        //static string _confidentialClientSecret = "qaxaDLJ39)uxuLHOP193[!^";
        //static string _confidentialClientRedirecURI = "msal52a3749f-4a89-456c-b762-e2bdb0216ecc://auth";

        // Registered app in AAD
        // Note /common - means any org or MSA (default)
        //      /organizations - means orgs only
        static string _authority = "https://login.microsoftonline.com/modernauthn.onmicrosoft.com";

        static string _apiResourceId = "https://demoapi";

        static string[] _apiGraphScopes = new string[] { "https://graph.microsoft.com/User.Read" };

        static string _publicClientId = "bee47179-c28d-4d1f-9607-da934eabb5da";
        static string[] _publicClientScopes = new string[] { _apiResourceId + "/reader" };

        static string _confidentialClientId = "46ed8e59-07c5-4539-9cbb-54a04834df4d";
        static string _confidentialClientSecret = "PDbD38vpnTJh7pFoUKdcDS3sJBfXk2MmrrpiUQitTvg=";
        static string _confidentialClientReplyUrl = "https://confidentialclient";

        static string _userName = "user1@modernauth.onmicrosoft.com";

        static void Main(string[] args)
        {
            var p = new Program();
            p.UseMSAL().Wait();
        }

        private async Task UseMSAL()
        {
            var pClient = PublicClientApplicationBuilder
                .Create(_publicClientId)
                .WithAuthority(_authority)
                .Build();
            IConfidentialClientApplication cClient;
            var exit = false;
            do
            {
                try
                {
                ShowMenu();
                var key = Console.ReadKey();
                switch (key.KeyChar)
                {
                    case '1':
                        Console.WriteLine("Auth Code Grant - public client");
                        ShowTokens(await pClient.AcquireTokenInteractive(_publicClientScopes).ExecuteAsync());
                        //ShowTokens(await pClient.AcquireTokenInteractive(new string[] { _apiResourceId + "/creator" }).ExecuteAsync());
                        break;
                    case 'S':
                            Console.WriteLine("Auth Code Grant - public client");
                            ShowTokens(await pClient.AcquireTokenSilent(_publicClientScopes, (await pClient.GetAccountsAsync()).First()).ExecuteAsync());
                            //ShowTokens(await pClient.AcquireTokenInteractive(new string[] { _apiResourceId + "/creator" }).ExecuteAsync());
                            break;
                    case '2':
                        Console.WriteLine("Client credentials (symetric)");
                        cClient = ConfidentialClientApplicationBuilder
                            .Create(_confidentialClientId)
                            .WithClientSecret(_confidentialClientSecret)
                            .WithAuthority(_authority)
                            .Build();
                        ShowTokens(await cClient.AcquireTokenForClient(new string[] { $"{_apiResourceId}/.default" }).ExecuteAsync());
                        break;
                    case '3':
                        Console.WriteLine("Client credentials (X509)");
                        cClient = ConfidentialClientApplicationBuilder
                            .Create(_confidentialClientId)
                            .WithCertificate(new X509Certificate2("./cert.pfx", "password"))
                            .WithTenantId("modernauthn.onmicrosoft.com")
                            .Build();
                        ShowTokens(await cClient.AcquireTokenForClient(new string[] { $"{_apiResourceId}/.default" }).ExecuteAsync());
                        break;
                    case '4':
                        Console.WriteLine("Resource owner password (public).");
                        var securePwd = GetPassword(_userName);
                        ShowTokens(await pClient.AcquireTokenByUsernamePassword(
                            _publicClientScopes,
                            _userName,
                            securePwd).ExecuteAsync());
                        break;
                    case '5':
                        Console.WriteLine("Not supported");
                        break;
                    case '6':
                        Console.WriteLine("On-behalf of");
                        // Public client calls the API with the access token
                        // Make sure accessTokenAcceptedVersion=2 in the API app's manifest!
                        pClient = PublicClientApplicationBuilder
                            .Create(_publicClientId)
                            .WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs)
                            .Build();
                        var apiTokens = await pClient.AcquireTokenInteractive(new string[] { "https://adalconsoleapi.com/user_impersonation" }).ExecuteAsync();
                        // Now API exchanges that token for another one to MS Graph
                        cClient = ConfidentialClientApplicationBuilder
                            .Create(_confidentialClientId)
                            .WithClientSecret(_confidentialClientSecret)
                            .WithAuthority(_authority)
                            .Build();
                        ShowTokens(await cClient.AcquireTokenOnBehalfOf(_apiGraphScopes,
                            new UserAssertion(apiTokens.AccessToken, "urn:ietf:params:oauth:grant-type:jwt-bearer")).ExecuteAsync());
                        break;
                    case '7':
                        Console.WriteLine("");
                        var pTokens = await pClient.AcquireTokenInteractive(_publicClientScopes).ExecuteAsync();
                        var saml = await AcquireSamlFromJWT(
                            _authority,
                            pTokens.AccessToken,
                            _confidentialClientId,
                            _confidentialClientSecret,
                            _apiResourceId,
                            _confidentialClientReplyUrl);
                        cClient = ConfidentialClientApplicationBuilder
                            .Create(_confidentialClientId)
                            .WithClientSecret(_confidentialClientSecret)
                            .WithAuthority(_authority)
                            .Build();
                        ShowTokens(await cClient.AcquireTokenOnBehalfOf(
                            _publicClientScopes,
                            new UserAssertion(saml, "")).ExecuteAsync());
                        break;
                    case '8':
                        Console.WriteLine("Device Code - public client");
                        ShowTokens(await pClient.AcquireTokenWithDeviceCode
                            (
                                _publicClientScopes,
                                result =>
                                {
                                    Console.WriteLine(result.Message);
                                    return Task.FromResult(0);
                                }).ExecuteAsync());
                        break;
                    case '9':
                        Console.WriteLine("Resource Owner using WIA (public)");
                        ShowTokens(await pClient.AcquireTokenByIntegratedWindowsAuth(_publicClientScopes).ExecuteAsync());
                        break;
                    case 'q':
                    case 'Q':
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid entry.");
                        break;
                }
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            } while (!exit);
        }
        private static void ShowMenu()
        {
            Console.WriteLine("Enter acquire token method:");
            Console.WriteLine("1. Auth Code Grant (public client).");
            Console.WriteLine("2. Client credentials - symetric.");
            Console.WriteLine("3. Client Credentials - X509.");
            Console.WriteLine("4. Resource owner.");
            Console.WriteLine("6. On-behalf of (confidential client).");
            Console.WriteLine("7. On-behalf of SAML (confidential client).");
            Console.WriteLine("8. Device code (public client).");
            Console.WriteLine("9. Resource owner using WIA (public client).");
            Console.WriteLine("Q. Quit");
        }
        private static void ShowTokens(AuthenticationResult result)
        {
            try
            {
                foreach (var p in result.GetType().GetProperties())
                {
                    Console.WriteLine($"{p.Name}: {p.GetValue(result)}");
                }
            } catch(Exception)
            {

            }
            Console.WriteLine();
        }

        private static SecureString GetPassword(string userName)
        {
            // NOTE: Resource owner, just like SecureString should not be used at all: https://github.com/dotnet/platform-compat/blob/master/docs/DE0001.md
            // Here is why. :)
            Console.Write($"Enter password for {userName}: ");
            var pwd = Console.ReadLine();
            var str = new SecureString();
            pwd.Select(c =>
            {
                str.AppendChar(c);
                return c;
            }).ToList();
            return str;
        }
        private static X509Certificate2 ReadCertificateFromStore(string thumbPrint)
        {
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var certCollection = store.Certificates;

            // Find unexpired certificates.
            var currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

            // From the collection of unexpired certificates, find the ones with the correct thumbprint.
            var signingCert = currentCerts.Find(X509FindType.FindByThumbprint, thumbPrint, false);

            // Return the first certificate in the collection, has the right name and is current.
            var cert = signingCert.OfType<X509Certificate2>().OrderByDescending(c => c.NotBefore).FirstOrDefault();
            store.Close();
            return cert;
        }
        /// <summary>
        /// Returns Base64 encoded SAML token using the AAD OBO exchange of a JWT token.
        /// </summary>
        /// <param name="auth"></param>
        /// <param name="jwtToken"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="resourceId"></param>
        /// <param name="replyUrl"></param>
        /// <returns></returns>
        private static async Task<string> AcquireSamlFromJWT(
            string authority,
            string jwtToken,
            string clientId,
            string clientSecret,
            string resourceId,
            string replyUrl
            )
        {

            var http = new HttpClient();
            var reqParams = new Dictionary<string, string>
            {
                {"grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
                {"assertion", jwtToken },
                {"client_id" , clientId },
                {"client_secret", HttpUtility.UrlEncode(clientSecret) },
                {"resource", HttpUtility.UrlEncode(resourceId) },
                {"redirect_uri", HttpUtility.UrlEncode(replyUrl) },
                {"requested_token_use", "on_behalf_of" },
                {"requested_token_type", "urn:ietf:params:oauth:token-type:saml2" }
            };
            var body = reqParams.Aggregate("", (s, v1) => $"{s}&{v1.Key}={v1.Value}").Substring(1); // skip initial &
            var resp = await http.PostAsync($"{authority}oauth2/token", new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"));
            if (resp.IsSuccessStatusCode)
            {
                var authResult = await resp.Content.ReadAsStringAsync();
                var saml = (string)JObject.Parse(authResult)["access_token"];
                return saml;
            }
            else
                throw new Exception(resp.ReasonPhrase);
        }
    }
}
