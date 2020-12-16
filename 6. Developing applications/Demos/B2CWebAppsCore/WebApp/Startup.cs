using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebApp.Extensions;

namespace WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(AzureADB2CDefaults.OpenIdScheme)
                .AddAzureADB2C(options => Configuration.Bind("AzureADB2C", options))
                .AddMSAL(options => Configuration.Bind("AzureADB2CClient", options));

            services.Configure<OpenIdConnectOptions>(AzureADB2CDefaults.OpenIdScheme, options =>
            {
                options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                options.Scope.Add("https://mrochonb2cprod.onmicrosoft.com/webapi/read_policies");
                options.Scope.Add("offline_access"); // otherwise no refresh token or acct created in cache
                options.Events.OnMessageReceived = async ctx =>
                {
                    // content already read, see ProtocolMessage
                    await Task.FromResult(0);
                };
                options.Events.OnAuthenticationFailed = async ctx =>
                {
                    // if AADB2C90118 send back for pwd reset
                    ctx.HandleResponse();
                    await Task.FromResult(0);
                };
                options.Events.OnRedirectToIdentityProvider = async ctx =>
                {
                    await Task.FromResult(0);
                };
                options.Events.OnTokenValidated = async ctx =>
                {
                    // ...
                    await Task.FromResult(0);
                };
                options.Events.OnAuthorizationCodeReceived = async ctx =>
                {
                    var sp = services.BuildServiceProvider();
                    var auth = sp.GetService<IConfidentialClientApplication>();
                    var scopes = new string[] {"https://mrochonb2cprod.onmicrosoft.com/webapi/read_policies" };
                    var tokens = await auth.AcquireTokenByAuthorizationCode(scopes, ctx.ProtocolMessage.Code).ExecuteAsync();
                    ctx.HandleCodeRedemption(ctx.ProtocolMessage.IdToken, ctx.ProtocolMessage.IdToken);
                };
            });
            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
