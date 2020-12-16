using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
using System.IdentityModel.Claims;
using Microsoft.Identity.Client;

namespace WebAppDemo
{
    public partial class Startup
    {
        public static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        public static string clientKey = ConfigurationManager.AppSettings["ida:Secret"];
        public static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        public static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        public static string[] scopes = { "https://webapidemo/admin" };
        public static string authority = $"https://login.microsoftonline.com/{tenantId}/V2.0";

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = authority,
                    ResponseType = "id_token code",
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        RedirectToIdentityProvider = async (args) =>
                        {
                            args.ProtocolMessage.PostLogoutRedirectUri = $"https://{args.Request.Uri.Host}:{args.Request.LocalPort}/account/signout";
                            await Task.FromResult(0);
                        },
                        AuthorizationCodeReceived = async (args) =>
                        {
                            var code = args.Code;
                            var userId = args.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;
                            var client = ConfidentialClientApplicationBuilder
                                .Create(clientId)
                                .WithClientSecret(clientKey)
                                .WithAuthority(authority)
                                .WithRedirectUri($"https://{args.Request.Uri.Host}:{args.Request.LocalPort}/signin-oidc")
                                .Build()
                                .WithSessionCache(new HttpContextWrapper(HttpContext.Current), userId);
                            var tokens = await (client.AcquireTokenByAuthorizationCode(scopes, code).ExecuteAsync());
                        }
                    }
                }) ; 
        }

        private static string EnsureTrailingSlash(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                return value + "/";
            }

            return value;
        }
    }
}
