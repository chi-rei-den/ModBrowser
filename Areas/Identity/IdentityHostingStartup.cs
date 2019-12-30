using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Chireiden.ModBrowser.Data;
using Chireiden.ModBrowser.Models;

[assembly: HostingStartup(typeof(Chireiden.ModBrowser.Areas.Identity.IdentityHostingStartup))]
namespace Chireiden.ModBrowser.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}