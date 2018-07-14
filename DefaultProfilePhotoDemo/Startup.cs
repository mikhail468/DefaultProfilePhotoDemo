using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefaultProfilePhotoDemo.Models;
using DefaultProfilePhotoDemo.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DefaultProfilePhotoDemo
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddScoped<IDataService<Profile>, DataService<Profile>>();
            //add identity services
            services.AddIdentity<IdentityUser, IdentityRole>
                (
                   config =>
                   {
                       config.Password.RequireDigit = false;
                       config.Password.RequiredLength = 3;
                       config.Password.RequireNonAlphanumeric = false;
                       config.Password.RequireUppercase = false;
                   }
                ).AddEntityFrameworkStores<MyDbContext>();
            services.AddDbContext<MyDbContext>();

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Account/Denied";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}
