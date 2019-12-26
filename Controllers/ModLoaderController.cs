﻿using Ionic.Zlib;
using Microsoft.AspNetCore.Mvc;
using ModBrowser.Data;
using ModBrowser.Models;
using ModBrowser.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Mime;

namespace ModBrowser.Controllers
{
    public class ModLoaderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ModLoaderController(ApplicationDbContext context)
        {
            this._context = context;
        }

        [HttpGet, HttpPost]
        public IActionResult Download(string Down)
        {
            var filename = Path.GetFileNameWithoutExtension(Down);
            return this._context.Mod.Find(filename) != null
                ? this.PhysicalFile($"./mods/{filename}.tmod", MediaTypeNames.Application.Octet, filename + ".tmod")
                : (IActionResult)this.NotFound();
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
        public IActionResult ModListing(string modloaderversion, string platform, string netversion)
        {
            var clientVersion = new Version(modloaderversion.Substring(12));
            var modlist = this._context.Mod.AsEnumerable().Select((m) =>
            {
                if (m.GetModLoaderVersion() <= clientVersion)
                {
                    m.ModLoaderVersion = null;
                }
                return m;
            });

            if (clientVersion <= new Version(0, 11))
            {
                return this.Json(new
                {
                    update = new
                    {
                        message = "An update to tModLoader is available.",
                        url = "https://github.com/tModLoader/tModLoader/releases"
                    },
                    modlist
                });
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
                    using (var stream = new GZipStream(ms, CompressionMode.Compress))
                    {
                        using (var sr = new StreamWriter(stream))
                        {
                            sr.Write(modlist);
                            sr.Flush();
                            encoded = Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }

                return clientVersion == SyncService.tModLoaderVersion
                    ? this.Json(new
                    {
                        modlist_compressed = modlist
                    })
                    : this.Json(new
                    {
                        update = new
                        {
                            message = "An update to tModLoader is available.",
                            url = "https://github.com/tModLoader/tModLoader/releases"
                        },
                        modlist_compressed = modlist
                    });
            }
        }
    }
}