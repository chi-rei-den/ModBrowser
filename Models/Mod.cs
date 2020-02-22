using Chireiden.ModBrowser.ModLoader;
using Chireiden.ModBrowser.Services;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Resources;
using System.Text.RegularExpressions;

namespace Chireiden.ModBrowser.Models
{
    public class Mod
    {
        [Key, JsonProperty("name"), Required]
        [Display(Name = "Name", ResourceType = typeof(Resources.Localization))]
        public string Name { get; set; }

        [JsonProperty("displayname")]
        [Display(Name = "DisplayName", ResourceType = typeof(Resources.Localization))]
        public string DisplayName { get; set; }

        [JsonProperty("version"), Required]
        [Display(Name = "Version", ResourceType = typeof(Resources.Localization))]
        public string Version { get; set; }

        [JsonProperty("author"), Required]
        [Display(Name = "Author", ResourceType = typeof(Resources.Localization))]
        public string Author { get; set; }

        [JsonProperty("updateTimeStamp")]
        [Display(Name = "UpdateTimeStamp", ResourceType = typeof(Resources.Localization))]
        public string UpdateTimeStamp { get; set; }

        [JsonProperty("description")]
        [Display(Name = "Description", ResourceType = typeof(Resources.Localization))]
        public string Description { get; set; }

        [JsonProperty("modloaderversion")]
        [Display(Name = "ModLoaderVersion", ResourceType = typeof(Resources.Localization))]
        public string ModLoaderVersion { get; set; }

        [JsonProperty("modreferences")]
        [Display(Name = "ModReferences", ResourceType = typeof(Resources.Localization))]
        public string ModReferences { get; set; }

        [JsonProperty("homepage")]
        [Display(Name = "Homepage", ResourceType = typeof(Resources.Localization))]
        public string Homepage { get; set; }

        [JsonProperty("iconurl")]
        [Display(Name = "IconURL", ResourceType = typeof(Resources.Localization))]
        public string IconURL { get; set; }

        [JsonProperty("modside")]
        [Display(Name = "ModSide", ResourceType = typeof(Resources.Localization))]
        public string ModSide { get; set; } = "Both";

        [JsonProperty("downloads")]
        [Display(Name = "Downloads", ResourceType = typeof(Resources.Localization))]
        public int Downloads { get; set; }

        [JsonProperty("hot")]
        [Display(Name = "Hot", ResourceType = typeof(Resources.Localization))]
        public int Hot { get; set; }

        [JsonProperty("size")]
        [Display(Name = "Size", ResourceType = typeof(Resources.Localization))]
        public int Size { get; set; }

        public Mod Clone()
        {
            return (Mod)this.MemberwiseClone();
        }
    }

    public static class ModHelper
    {
        public static readonly Regex Format = new Regex("(?<!\\\\)\\[(?<tag>[a-zA-Z]{1,10})(\\/(?<options>[^:]+))?:(?<text>.+?)(?<!\\\\)\\]", RegexOptions.Compiled);

        public static Version GetVersion(this Mod mod)
        {
            return Version.TryParse(mod.Version?.Substring(1), out var result) ? result : new Version(1, 0);
        }

        public static Version GetModLoaderVersion(this Mod mod)
        {
            return Version.TryParse(mod.ModLoaderVersion?.Substring(12), out var result) ? result : SyncService.tModLoaderVersion;
        }

        public static DateTime GetUpdateTimestamp(this Mod mod)
        {
            return DateTime.TryParse(mod.UpdateTimeStamp, out var result) ? result : DateTime.Now;
        }

        public static string FilePath(this Mod mod)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mods", mod.Name + ".tmod");
        }

        public static string IconPath(this Mod mod)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mods", mod.Name + ".png");
        }

        public static string ModSideEnum(this Mod mod)
        {
            var displayAttribute = typeof(ModSide).GetMember(mod.ModSide)[0].GetCustomAttributes(typeof(DisplayAttribute), false)[0] as DisplayAttribute;
            return new ResourceManager(displayAttribute.ResourceType).GetString(displayAttribute.Name);
        }

        public static string TagToHtml(this string value)
        {
            return Format.Replace(System.Net.WebUtility.HtmlEncode(value), (m) =>
            {
                return m.Groups["tag"].Value switch
                {
                    "c" => $"<span style=\"color:#{m.Groups["options"]}\">{m.Groups["text"].Value}</span>",
                    "i" => $"<img src=\"direct/icons/{m.Groups["text"].Value}.png\"/>",
                    _ => m.Value
                };
            }).Replace("\r\n", "<br />").Replace("\r", "<br />").Replace("\n", "<br />");
        }
    }
}
