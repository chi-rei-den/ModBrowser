using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModBrowser.Data;
using ModBrowser.Services;

namespace ModBrowser
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite("Data Source=.\\wwwroot\\sqlite.db"));
            services.AddDefaultIdentity<IdentityUser>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, SyncService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // app.UseHsts();
            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "tModLoaderDownload",
                    template: "tModLoader/download.php",
                    defaults: new { controller = "ModLoader", action = "Download" });
                routes.MapRoute(
                    name: "tModLoaderQueryDesc",
                    template: "tModLoader/tools/querymodnamehomepagedescription.php",
                    defaults: new { controller = "ModLoader", action = "HomepageAndDescription" });
                routes.MapRoute(
                    name: "tModLoaderQueryURL",
                    template: "tModLoader/tools/querymoddownloadurl.php",
                    defaults: new { controller = "ModLoader", action = "GetDownloadURL" });
                routes.MapRoute(
                    name: "tModLoaderModInfo",
                    template: "tModLoader/tools/modinfo.php",
                    defaults: new { controller = "ModLoader", action = "ModInfo" });
                routes.MapRoute(
                    name: "tModLoaderModVersion",
                    template: "tModLoader/tools/latestmodversion.php",
                    defaults: new { controller = "ModLoader", action = "ModVersion" });
                routes.MapRoute(
                    name: "tModLoaderModVersionSimple",
                    template: "tModLoader/tools/latestmodversionsimple.php",
                    defaults: new { controller = "ModLoader", action = "ModVersionSimple" });
                routes.MapRoute(
                    name: "tModLoaderModHomepage",
                    template: "tModLoader/tools/querymodhomepage.php",
                    defaults: new { controller = "ModLoader", action = "ModHomepage" });
                routes.MapRoute(
                    name: "tModLoaderModDescription",
                    template: "tModLoader/moddescription.php",
                    defaults: new { controller = "ModLoader", action = "ModDescription" });
                routes.MapRoute(
                    name: "tModLoaderModListing",
                    template: "tModLoader/listmods.php",
                    defaults: new { controller = "ModLoader", action = "ModListing" });
                routes.MapRoute(
                    name: "tModLoaderDirectModListing",
                    template: "tModLoader/DirectModDownloadListing.php",
                    defaults: new { controller = "Mods", action = "Index" });
            });
        }
    }
}
