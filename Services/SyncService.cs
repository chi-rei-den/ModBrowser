using Ionic.Zlib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModBrowser.Data;
using ModBrowser.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ModBrowser.Services
{
    public class SyncService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        internal static HttpClient Http = new HttpClient() { Timeout = new TimeSpan(0, 5, 00) };
        internal static Version tModLoaderVersion = new Version("0.11.5");

        public SyncService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Directory.Exists("mods"))
            {
                Directory.CreateDirectory("mods");
            }

            while (true)
            {
                string str;
                try
                {
                    str = await Http.PostAsync("http://javid.ddns.net/tModLoader/listmods.php", new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["modloaderversion"] = tModLoaderVersion.ToString(),
                        ["platform"] = "w",
                        ["netversion"] = "4.0",
                    })).Result.Content.ReadAsStringAsync();
                }
                catch
                {
                    continue;
                }

                JObject json;
                try
                {
                    json = (JObject)JsonConvert.DeserializeObject(str);
                }
                catch
                {
                    continue;
                }

                if (json.ContainsKey("update"))
                {
                    tModLoaderVersion = new Version(json["update"].ToString());
                    continue;
                }

                if (!json.ContainsKey("modlist_compressed"))
                {
                    continue;
                }

                string list;
                using (var ms = new MemoryStream(Convert.FromBase64String(json["modlist_compressed"].ToString())))
                {
                    using (var stream = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        using (var sr = new StreamReader(stream))
                        {
                            list = sr.ReadToEnd();
                        }
                    }
                }

                var modlist = JsonConvert.DeserializeObject<List<Mod>>(list);

                var descriptions = Http.GetStringAsync("http://javid.ddns.net/tModLoader/tools/querymodnamehomepagedescription.php").Result;
                var desclist = JsonConvert.DeserializeObject<List<Mod>>(descriptions).ToDictionary(i => i.Name);
                foreach (var item in modlist)
                {
                    item.Homepage = !string.IsNullOrWhiteSpace(desclist[item.Name].Homepage) ? desclist[item.Name].Homepage : null;
                    item.Description = !string.IsNullOrWhiteSpace(desclist[item.Name].Description) ? desclist[item.Name].Description : null;
                }

                Parallel.ForEach(modlist, new ParallelOptions
                {
                    MaxDegreeOfParallelism = 2
                }, async item =>
                {
                    try
                    {
                        using (var scope = this.scopeFactory.CreateScope())
                        {
                            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            var found = db.Mod.Find(item.Name);
                            if (found == null)
                            {
                                var result = await Http.GetByteArrayAsync($"http://javid.ddns.net/tModLoader/download.php?Down=mods/{item.Name}.tmod");
                                File.WriteAllBytes($"./mods/{item.Name}.tmod", result);
                                db.Mod.Add(item);
                                db.SaveChanges();
                            }
                            else
                            {
                                if (found.Version != item.Version)
                                {
                                    var result = await Http.GetByteArrayAsync($"http://javid.ddns.net/tModLoader/download.php?Down=mods/{item.Name}.tmod");
                                    File.WriteAllBytes($"./mods/{item.Name}.tmod", result);
                                }
                                db.Entry(found).CurrentValues.SetValues(item);
                                db.Mod.Update(found);
                                db.SaveChanges();
                            }
                        }
                    }
                    catch
                    {
                    }
                });

                await Task.Delay(TimeSpan.FromMinutes(30));
            }
        }

        private static void ExtractInfo(byte[] file, Mod mod)
        {
            using (var ms = new MemoryStream(file))
            {
                using (var br = new BinaryReader(ms))
                {
                    br.ReadBytes(4);
                    mod.ModLoaderVersion = "tModLoader v" + br.ReadString();
                }
            }
        }
    }
}
