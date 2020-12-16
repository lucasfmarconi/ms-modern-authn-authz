using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp.Extensions
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddMSAL(this AuthenticationBuilder app, Action<ConfidentialClientApplicationOptions> configureOptions)
        {
            var options = new ConfidentialClientApplicationOptions();
            configureOptions(options);
            var authority = $"https://mrochonb2cprod.b2clogin.com/tfp/{options.TenantId}/B2C_1_BasicSUSI";
            var auth = ConfidentialClientApplicationBuilder
                .CreateWithApplicationOptions(options)
                .WithB2CAuthority(authority)
                .WithRedirectUri("https://localhost:44316")
                .Build();
            app.Services.AddSingleton<IConfidentialClientApplication>(auth);
            return app;
        }
    }
}
