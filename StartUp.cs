using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CommonHelper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using AppMvc.Services.Mail;
using AppMvc.Common;
using AppMvc.Models;
using AppMvc.Security;

namespace AppMvc
{
    public class Startup
    {
        private Dictionary<string, string> strConnList = new Dictionary<string, string>();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddRazorPages();
            services.AddControllersWithViews();

            services.AddOptions();
            var mailconfig = Configuration.GetSection("MailSettings");
            services.Configure<MailConfig>(mailconfig);
            services.AddSingleton<IEmailSender, SendMailService>();
            services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();
            services.AddDbContext<AppDbContext>(options =>
            {
                string connectionString = Configuration.GetConnectionString("AppDbContext");
                string conStr = string.Empty;

                if (!strConnList.ContainsKey(connectionString))
                {
                    conStr = connectionString.MyDecrypt("");
                    strConnList.Add(connectionString, conStr);
                }
                else
                {
                    conStr = strConnList[connectionString];
                }

                options.UseSqlServer(conStr);
            });

            services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            //  services.AddDefaultIdentity<AppUser>()
            // .AddEntityFrameworkStores<AppDbContext>();
            // .AddDefaultTokenProviders();
            services.Configure<IdentityOptions>(options =>
            {
                // Password configurations
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 3;
                options.Password.RequiredUniqueChars = 1;

                //User lockout configurations
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // lock in 5 minutes
                options.Lockout.MaxFailedAccessAttempts = 5; // Locking if wrong access 5 times
                options.Lockout.AllowedForNewUsers = true;

                // User configurations.
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                // Login configurations
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedAccount = true;

            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login";
                options.LogoutPath = "/logoff";
                options.AccessDeniedPath = "/Accessdenied";
            });

            services.AddAuthorization(options => {
                options.AddPolicy("App", policyBuilder => {
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.Requirements.Add(new AppRequirement());
                });
            });

            services.AddTransient<IAuthorizationHandler, AppAuthorizationHandler>();
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
            //app.UseMiddleware<LoggingMiddleWare>();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapAreaControllerRoute(
                    name: "Admin",
                    areaName: "Identity",
                    pattern: "/{controller=Home}/{action=Index}/{id?}"
                );
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");


                //endpoints.MapRazorPages();
            });
        }
    }
}
