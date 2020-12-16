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
using Microsoft.Identity.Client;

namespace B2CDemo
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string secret = ConfigurationManager.AppSettings["ida:Key"];
        private static string aadInstance = EnsureTrailingSlash(ConfigurationManager.AppSettings["ida:AADInstance"]);
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        private static string authority = aadInstance + tenantId;

        public static IConfidentialClientApplication MSALApp = null;

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    MetadataAddress = "https://mrochonb2cprod.b2clogin.com/cf6c572c-c72e-4f31-bd0b-75623d040495/b2c_1_basicsusi/v2.0/.well-known/openid-configuration",
                    RedirectUri = "https://localhost:44353/",
                    PostLogoutRedirectUri = postLogoutRedirectUri,
                    Scope = "offline_access openid https://mrochonb2cprod.onmicrosoft.com/demoapi/read",
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        
                        AuthorizationCodeReceived = async (ctx) =>
                        {
                            var code = ctx.Code;
                            MSALApp = ConfidentialClientApplicationBuilder
                                .Create(clientId)
                                .WithB2CAuthority("https://mrochonb2cprod.b2clogin.com/tfp/cf6c572c-c72e-4f31-bd0b-75623d040495/b2c_1_basicsusi/")
                                .WithClientSecret(secret)
                                .WithRedirectUri("https://localhost:44353/")
                                .Build();
                            
                            new UserCacheProvider(MSALApp.UserTokenCache, HttpContext.Current.Session);
                            
                            var tokens = await MSALApp.AcquireTokenByAuthorizationCode(
                                new string[] { "https://mrochonb2cprod.onmicrosoft.com/demoapi/read" },
                                code).ExecuteAsync();
                            var accts = await MSALApp.GetAccountsAsync();
                        },
                    }
                });
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
