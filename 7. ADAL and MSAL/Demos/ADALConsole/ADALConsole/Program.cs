using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace ADALConsole
{
    class Program
    {

        // Configuration data:
        // Public client gets tokens to call an API (confidential client)
        // Confidential client exchanges these tokens for other resources or token types

        static string _authority = "https://login.microsoftonline.com/modernauthn.onmicrosoft.com";

        static string _publicClientId = "bee47179-c28d-4d1f-9607-da934eabb5da";
        static string _publicClientReplyUrl = "urn:ietf:wg:oauth:2.0:oob";

        static string _confidentialClientId = "46ed8e59-07c5-4539-9cbb-54a04834df4d";
        static string _confidentialClientSecret = "PDbD38vpnTJh7pFoUKdcDS3sJBfXk2MmrrpiUQitTvg=";
        static string _clientCertThumbprint = "CEC570D98C116EFB9E425E8E888120F3558ECE37";
        static string _apiReplyUrl = "https://adalconsole.com";
        static string _apiResourceId = "https://adalconsoleapi.com";

        static string _graphResourceId = "https://graph.microsoft.com";

        static void Main(string[] args)
        {
            var p = new Program();

            AuthenticationResult tokens = null;
            var exit = false;
            do
            {
                ShowMenu();
                var key = Console.ReadKey();
                switch(key.KeyChar)
                {
                    case '1':
                        tokens = p.PublicAuthCodeGrant().Result;
                        ShowTokens("Auth Code Grant", tokens);
                        break;
                    case '2':
                        ShowTokens("Client credentials (symetric)", p.ClientCredentialsSymmetric().Result); break;
                    case '3':
                        ShowTokens("Client credentials (X509)", p.ClientCredentialsX509().Result); break;
                    case '4':
                        ShowTokens("Resource owner", p.ResourceOwner().Result); break;
                    case '5':
                        if (tokens == null)
                        {
                            Console.WriteLine("Using Auth Code Grant to get user tokens first");
                            tokens = p.PublicAuthCodeGrant().Result;
                        }
                        ShowTokens("OBO (public client)", p.PublicGetOboJWT(tokens).Result);
                        break;
                    case '6':
                        if (tokens == null)
                        {
                            Console.WriteLine("Using Auth Code Grant to get user tokens first");
                            tokens = p.PublicAuthCodeGrant().Result;
                        }
                        ShowTokens("OBO (confidential client)", p.ConfidentialGetOboJWT(tokens).Result);
                        break;
                    case '7':
                        if (tokens == null)
                        {
                            Console.WriteLine("Using Auth Code Grant to get user tokens first");
                            tokens = p.PublicAuthCodeGrant().Result;
                        }
                        Console.WriteLine("OBO SAML (confidential client)");
                        Console.WriteLine(p.ConfidentialGetOboSAML(tokens).Result);
                        break;
                    case '8':
                        ShowTokens("Non UI user device using device code", p.UseDeviceCodeAsync().Result);
                        break;
                    case 'q':
                    case 'Q':
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid entry.");
                        break;
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
            Console.WriteLine("(For OBO calls, please execute 1 (Auth Code Grant) at least once)");
            Console.WriteLine("5. On-behalf of (public client).");
            Console.WriteLine("6. On-behalf of (confidential client).");
            Console.WriteLine("7. On-behalf of SAML (confidential client).");
            Console.WriteLine("8. Device code flow.");
            Console.WriteLine("Q. Quit");
        }
        private async Task<AuthenticationResult> PublicAuthCodeGrant()
        {
            var auth = new AuthenticationContext(_authority);
            return await auth.AcquireTokenAsync(
                _apiResourceId,
                _publicClientId,
                new Uri(_publicClientReplyUrl),
                new PlatformParameters(PromptBehavior.Auto));
        }
        private async Task<AuthenticationResult> ClientCredentialsSymmetric()
        {
            var auth = new AuthenticationContext(_authority);
            return await auth.AcquireTokenAsync(
                _apiResourceId,
                new ClientCredential(_confidentialClientId, _confidentialClientSecret));
        }
        private async Task<AuthenticationResult> ClientCredentialsX509()
        {
            var auth = new AuthenticationContext(_authority);
            var certCred = new ClientAssertionCertificate(_confidentialClientId, ReadCertificateFromStore(_clientCertThumbprint));
            return await auth.AcquireTokenAsync(
                _apiResourceId,
                certCred);
        }
        private async Task<AuthenticationResult> ResourceOwner()
        {
            var auth = new AuthenticationContext(_authority);
            return await auth.AcquireTokenAsync(
                _apiResourceId,
                _publicClientId,
                new UserPasswordCredential("user1@modernauthn.onmicrosoft.com", "SoftPr04you"));
        }
        private async Task<AuthenticationResult> PublicGetOboJWT(AuthenticationResult tokens)
        {
            var auth = new AuthenticationContext(_authority);
            return await auth.AcquireTokenAsync(
                _graphResourceId,
                _publicClientId,
                new UserAssertion(tokens.AccessToken, "urn:ietf:params:oauth:grant-type:jwt-bearer")
                );
        }
        private async Task<AuthenticationResult> ConfidentialGetOboJWT(AuthenticationResult tokens)
        {
            var auth = new AuthenticationContext(_authority);
            return await auth.AcquireTokenAsync(
                _graphResourceId,
                new ClientCredential(_confidentialClientId, _confidentialClientSecret),
                new UserAssertion(tokens.AccessToken, "urn:ietf:params:oauth:grant-type:jwt-bearer")
                );
        }
        private async Task<string> ConfidentialGetOboSAML(AuthenticationResult tokens)
        {
            Console.WriteLine("Confidential client OBO JWT->SAML");
            var auth = new AuthenticationContext(_authority);
            return await auth.AcquireSamlFromJWT(
                tokens.AccessToken, 
                _confidentialClientId, 
                _confidentialClientSecret, 
                _graphResourceId, 
                _apiReplyUrl);
        }
        private async Task<AuthenticationResult> UseDeviceCodeAsync()
        {
            var auth = new AuthenticationContext(_authority);
            var code = await auth.AcquireDeviceCodeAsync(_graphResourceId, _publicClientId);
            Console.WriteLine(code.Message);
            return await auth.AcquireTokenByDeviceCodeAsync(code);
        }

        private static void ShowTokens(string title, AuthenticationResult result)
        {
            Console.WriteLine(title);
            foreach (var p in result.GetType().GetProperties())
            {
                Console.WriteLine($"{p.Name}: {p.GetValue(result)}");
            }
            Console.WriteLine();
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
    }
}
