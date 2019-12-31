using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Chireiden.ModBrowser.Areas.Identity.IdentityHostingStartup))]
namespace Chireiden.ModBrowser.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}