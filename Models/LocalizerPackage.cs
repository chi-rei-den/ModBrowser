using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Chireiden.ModBrowser.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LocalizerPackage
    {
        [Key, JsonProperty("id"), Required]
        public int Id { get; set; }

        [JsonProperty("name")]
        [Display(Name = "DisplayName", ResourceType = typeof(Resources.Localization))]
        public string Name { get; set; }

        [JsonProperty("author"), Required]
        [Display(Name = "Author", ResourceType = typeof(Resources.Localization))]
        public string Author { get; set; }

        [JsonProperty("version"), Required]
        [Display(Name = "Version", ResourceType = typeof(Resources.Localization))]
        public string Version { get; set; }

        [JsonProperty("description")]
        [Display(Name = "Description", ResourceType = typeof(Resources.Localization))]
        public string Description { get; set; }

        [JsonProperty("language")]
        [Display(Name = "Language", ResourceType = typeof(Resources.Localization))]
        public string Language { get; set; }

        [JsonProperty("created_at")]
        [Display(Name = "CreateTimeStamp", ResourceType = typeof(Resources.Localization))]
        public string CreateTimeStamp { get; set; }

        [JsonProperty("updated_at")]
        [Display(Name = "UpdateTimeStamp", ResourceType = typeof(Resources.Localization))]
        public string UpdateTimeStamp { get; set; }

        [JsonProperty("modversion")]
        [Display(Name = "ModVersion", ResourceType = typeof(Resources.Localization))]
        public string ModVersion { get; set; }

        [JsonProperty("mod")]
        [Display(Name = "ModName", ResourceType = typeof(Resources.Localization))]
        public string ModName { get; set; }

        public Mod Mod { get; set; }

        public string UploaderId { get; set; }

        public ApplicationUser Uploader { get; set; }
    }

    public class PackageFile
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string ModName { get; set; }
        public string LocalizedModName { get; set; }
        public string Description { get; set; }
        public Version Version { get; set; }
        public Version ModVersion { get; set; }
        public string Language { get; set; }

        public static PackageFile FromFile(byte[] package)
        {
            using (var zipFile = new MemoryStream(package))
            {
                using (var archive = new ZipArchive(zipFile, ZipArchiveMode.Read))
                {
                    var packageFile = archive.GetEntry("Package.json")?.Open();
                    using (var sr = new StreamReader(packageFile))
                    {
                        var content = sr.ReadToEnd();
                        try
                        {
                            return JsonConvert.DeserializeObject<PackageFile>(content, new VersionConverter());
                        }
                        catch
                        {
                            return JsonConvert.DeserializeObject<PackageFile>(content);
                        }
                    }
                }
            }
        }

        public void CopyTo(LocalizerPackage package)
        {
            package.Author = this.Author;
            package.Description = this.Description;
            package.Language = this.Language;
            package.ModName = this.ModName;
            package.ModVersion = this.ModVersion.ToString();
            package.Name = this.Name;
            package.Version = this.Version.ToString();
        }
    }

    public static class PackageHelper
    {
        public static string FilePath(this LocalizerPackage package)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mods", package.Id + ".locpack");
        }
    }
}