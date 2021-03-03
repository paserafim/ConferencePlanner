using FrontEnd.Areas.Identity;
using FrontEnd.Areas.Identity.Data;
using FrontEnd.Data;
using FrontEnd.HealthChecks;
using FrontEnd.Middleware;
using FrontEnd.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrontEnd
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
            services.AddHealthChecks().AddDbContextCheck<IdentityDbContext>();

            services.AddHealthChecks()
                                    .AddCheck<BackendHealthCheck>("backend")
                                    .AddDbContextCheck<IdentityDbContext>();

            services.AddDefaultIdentity<User>()
                    .AddEntityFrameworkStores<IdentityDbContext>()
                    .AddClaimsPrincipalFactory<ClaimsPrincipalFactory>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                {
                    policy.RequireAuthenticatedUser()
                          .RequireIsAdminClaim();
                });
            });

            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Admin", "Admin");
            });

            services.AddSingleton<IAdminService, AdminService>();
            services.AddRazorPages();
            services.AddHttpClient<IApiClient, ApiClient>(client =>
                {
                    // this will point to our BackEnd API application
                    client.BaseAddress = new Uri(Configuration["ServiceUrl"]);
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<RequireLoginMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHealthChecks("/health");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
