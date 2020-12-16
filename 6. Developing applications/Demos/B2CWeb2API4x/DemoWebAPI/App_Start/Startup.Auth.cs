using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using Microsoft.IdentityModel.Tokens; 
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

namespace DemoWebAPI
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            var tvps = new TokenValidationParameters
            {
                // Accept only those tokens where the audience of the token is equal to the client ID of this app
                ValidAudience = ConfigurationManager.AppSettings["ida:Audience"],
                AuthenticationType = OpenIdConnectAuthenticationDefaults.AuthenticationType
            };

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions()
            {
                // This SecurityTokenProvider fetches the Azure AD B2C metadata & signing keys from the OpenIDConnect metadata endpoint
                AccessTokenFormat = new JwtFormat(tvps,
                    new OpenIdConnectCachingSecurityTokenProvider($"https://mrochonb2cprod.b2clogin.com/mrochonb2cprod.onmicrosoft.com/b2c_1_basicsusi/v2.0/.well-known/openid-configuration"))
            });
        }
    }
}
