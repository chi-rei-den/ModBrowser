using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModBrowser.Data;
using ModBrowser.Models;

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
            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "tModLoaderDownload",
                    pattern: "tModLoader/download.php",
                    defaults: new { controller = "ModLoader", action = "Download" });
                endpoints.MapControllerRoute(
                    name: "tModLoaderQueryDesc",
                    pattern: "tModLoader/tools/querymodnamehomepagedescription.php",
                    defaults: new { controller = "ModLoader", action = "HomepageAndDescription" });
                endpoints.MapControllerRoute(
                    name: "tModLoaderQueryURL",
                    pattern: "tModLoader/tools/querymoddownloadurl.php",
                    defaults: new { controller = "ModLoader", action = "GetDownloadURL" });
                endpoints.MapControllerRoute(
                    name: "tModLoaderModInfo",
                    pattern: "tModLoader/tools/modinfo.php",
                    defaults: new { controller = "ModLoader", action = "ModInfo" });
                endpoints.MapControllerRoute(
                    name: "tModLoaderModVersion",
                    pattern: "tModLoader/tools/latestmodversion.php",
                    defaults: new { controller = "ModLoader", action = "ModVersion" });
                endpoints.MapControllerRoute(
                    name: "tModLoaderModVersionSimple",
                    pattern: "tModLoader/tools/latestmodversionsimple.php",
                    defaults: new { controller = "ModLoader", action = "ModVersionSimple" });
                endpoints.MapControllerRoute(
                    name: "tModLoaderModHomepage",
                    pattern: "tModLoader/tools/querymodhomepage.php",
                    defaults: new { controller = "ModLoader", action = "ModHomepage" });
                endpoints.MapControllerRoute(
                    name: "tModLoaderModDescription",
                    pattern: "tModLoader/moddescription.php",
                    defaults: new { controller = "ModLoader", action = "ModDescription" });
                endpoints.MapControllerRoute(
                    name: "tModLoaderModListing",
                    pattern: "tModLoader/listmods.php",
                    defaults: new { controller = "ModLoader", action = "ModListing" });
                endpoints.MapControllerRoute(
                    name: "tModLoaderDirectModListing",
                    pattern: "tModLoader/DirectModDownloadListing.php",
                    defaults: new { controller = "Mods", action = "Index" });
                endpoints.MapRazorPages();
            });
        }
    }
}
