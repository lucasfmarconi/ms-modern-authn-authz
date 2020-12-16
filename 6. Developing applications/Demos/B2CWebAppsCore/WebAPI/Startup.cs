using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebAPI
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
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    Configuration.Bind("JwtBearer", options);
                    //options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    //{
                    //    RoleClaimType = "scp"
                    //};
                    //options.Events = new JwtBearerEvents()
                    //{
                    //    OnMessageReceived = async (ctx) =>
                    //    {
                    //        await Task.FromResult(0);
                    //    },
                    //    OnChallenge = async (ctx) =>
                    //    {
                    //        await Task.FromResult(0);
                    //        ctx.HandleResponse();
                    //    },
                    //    OnAuthenticationFailed = async (ctx) =>
                    //    {
                    //        await Task.FromResult(0);
                    //    },
                    //    OnTokenValidated = async (ctx) =>
                    //    {
                    //        await Task.FromResult(0);
                    //    }
                    //};
                });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // If you don't include this before UseMvc, bearer validation will fail with a challenge!
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
