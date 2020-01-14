using Chireiden.ModBrowser.Data;
using Chireiden.ModBrowser.Models;
using Chireiden.ModBrowser.Services;
using Ionic.Zlib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;

namespace Chireiden.ModBrowser.Controllers
{
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public class ModLoaderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ModLoaderController> _logger;

        public ModLoaderController(ApplicationDbContext context, ILogger<ModLoaderController> logger)
        {
            this._context = context;
            this._logger = logger;
        }

        [HttpGet, HttpPost]
        public IActionResult Download(string Down)
        {
            var filename = Path.GetFileNameWithoutExtension(Down);
            var mod = this._context.Mod.Find(filename);
            if (mod != null)
            {
                return this.PhysicalFile(mod.FilePath(), MediaTypeNames.Application.Octet, $"{mod.Name}_{mod.Version}.tmod");
            }
            else
            {
                IEnumerable<Mod> result = this._context.Mod;
                var search = Down.ToLower();
                var found = result.Where(r => r.Name.ToLower().Contains(search)).ToList();
                if (found.Count == 1)
                {
                    return this.PhysicalFile(found[0].FilePath(), MediaTypeNames.Application.Octet, $"{found[0].Name}_{found[0].Version}.tmod");
                }
                else if (found.Count > 1)
                {
                    return this.Content(string.Join(", ", found.Select(i => i.Name)));
                }
                return this.NotFound();
            }
        }

        [HttpGet]
        public IActionResult HomepageAndDescription()
        {
            return this.Json(this._context.Mod.Select(m => new
            {
                name = m.Name,
                homepage = m.Homepage,
                description = m.Description
            }));
        }

        [HttpGet, HttpPost]
        public IActionResult GetDownloadURL(string modname)
        {
            return this.Content($"{this.Request.Scheme}://{this.Request.Host}/tModLoader/download.php?Down=mods/{modname}.tmod");
        }

        [HttpGet, HttpPost]
        public IActionResult ModInfo(string modname)
        {
            var result = this._context.Mod.Find(modname);
            return result != null ? this.Json(result) : (IActionResult)this.NotFound();
        }

        [HttpGet, HttpPost]
        public IActionResult ModVersion(string modname)
        {
            var result = this._context.Mod.Find(modname);
            return result != null ? this.Json(new { version = result.Version }) : (IActionResult)this.NotFound();
        }

        [HttpGet, HttpPost]
        public IActionResult ModVersionSimple(string modname)
        {
            var result = this._context.Mod.Find(modname);
            return result != null ? this.Content(result.Version) : (IActionResult)this.NotFound();
        }

        [HttpGet, HttpPost]
        public IActionResult ModHomepage(string modname)
        {
            var result = this._context.Mod.Find(modname);
            return result != null ? this.Content(result.Homepage) : (IActionResult)this.NotFound();
        }

        [HttpGet, HttpPost]
        public IActionResult ModDescription(string modname)
        {
            var result = this._context.Mod.Find(modname);
            return result != null ? this.Json(new
            {
                homepage = result.Homepage,
                description = result.Description
            }) : (IActionResult)this.NotFound();
        }

        [HttpGet, HttpPost]
        public IActionResult ModListing(string modloaderversion, string platform, string netversion, string uncompressed, string preserveicon)
        {
            platform ??= "w";
            var updateurl = platform[0] switch
            {
                'w' => $"tModLoader.Windows.v{SyncService.tModLoaderVersion}.zip",
                'l' => $"tModLoader.Linux.v{SyncService.tModLoaderVersion}.tar.gz",
                'm' => $"tModLoader.Mac.v{SyncService.tModLoaderVersion}.zip",
                _ => null
            };

            var clientVersion = new Version(!string.IsNullOrWhiteSpace(modloaderversion) && modloaderversion.Length > 12
                ? modloaderversion.Substring(12)
                : "0.0.0.0");
            var mirrorIcon = string.IsNullOrWhiteSpace(preserveicon);
            var modlist = this._context.Mod.AsEnumerable().Select((m) =>
            {
                var cloned = m.Clone();
                if (cloned.GetModLoaderVersion() <= clientVersion)
                {
                    cloned.ModLoaderVersion = null;
                }
                if (mirrorIcon)
                {
                    cloned.IconURL = System.IO.File.Exists(cloned.IconPath()) ? $"{this.Request.Scheme}://{this.Request.Host}/direct/{cloned.Name}.png" : null;
                }
                return cloned;
            }).ToList();

            this._logger.LogInformation($"ModList: {modlist.Count} items.");

            if (clientVersion <= new Version(0, 11) || !string.IsNullOrWhiteSpace(uncompressed))
            {
                return this.Content(JsonConvert.SerializeObject(new
                {
                    update = new
                    {
                        message = "An update to tModLoader is available.",
                        url = "https://github.com/tModLoader/tModLoader/releases",
                        autoupdateurl = $"{this.Request.Scheme}://{this.Request.Host}/direct/{updateurl}",
                        current = clientVersion.ToString(),
                    },
                    modlist
                }, Formatting.None, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }), MediaTypeNames.Application.Json);
            }
            else
            {
                var serialized = JsonConvert.SerializeObject(modlist, Formatting.None, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                string encoded;
                using (var ms = new MemoryStream())
                {
                    using (var stream = new GZipStream(ms, CompressionMode.Compress, true))
                    {
                        using var sr = new StreamWriter(stream);
                        sr.Write(serialized);
                        sr.Flush();
                    }

                    encoded = Convert.ToBase64String(ms.ToArray());
                    this._logger.LogInformation($"ModList: Compress {serialized.Length} to {encoded.Length}.");
                }

                return this.Content(JsonConvert.SerializeObject(
                    new
                    {
                        update = clientVersion == SyncService.tModLoaderVersion
                        ? null : new
                        {
                            message = "An update to tModLoader is available.",
                            url = "https://github.com/tModLoader/tModLoader/releases",
                            autoupdateurl = $"{this.Request.Scheme}://{this.Request.Host}/direct/{updateurl}",
                        },
                        modlist_compressed = encoded
                    }, Formatting.None, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }), MediaTypeNames.Application.Json);
            }
        }

        // TODO: unpublishmymod.php
        [HttpGet, HttpPost]
        public IActionResult Unpublish(string name, string steamid64, string modloaderversion, string passphrase)
        {
            throw new NotImplementedException();
        }

        // TODO: listmymods.php
        [HttpGet, HttpPost]
        public IActionResult Owned(string steamid64, string modloaderversion, string passphrase)
        {
            throw new NotImplementedException();
        }

        // TODO: publishmod.php
        [HttpGet, HttpPost]
        public IActionResult Publish(string displayname, string displaynameclean, string name, string version, string author, string homepage, string description, string steamid64, string modloaderversion, string passphrase, string modreferences, string modside)
        {
            throw new NotImplementedException();
        }
    }
}