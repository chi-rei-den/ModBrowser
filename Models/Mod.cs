﻿using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ModBrowser.Models
{
    public class Mod
    {
        [Key, JsonProperty("name"), Required]
        public string Name { get; set; }

        [JsonProperty("displayname"), DisplayName("Name")]
        public string DisplayName { get; set; }

        [JsonProperty("version"), Required]
        public string Version { get; set; }

        [JsonProperty("author"), Required]
        public string Author { get; set; }

        [JsonProperty("updateTimeStamp"), DisplayName("Last Updated")]
        public string UpdateTimeStamp { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("modloaderversion")]
        public string ModLoaderVersion { get; set; }

        [JsonProperty("modreferences")]
        public string ModReferences { get; set; } = "";

        [JsonProperty("homepage")]
        public string Homepage { get; set; }

        [JsonProperty("iconurl")]
        public string IconURL { get; set; }

        [JsonProperty("modside")]
        public string ModSide { get; set; } = "Both";

        [JsonProperty("downloads")]
        public int Downloads { get; set; }

        [JsonProperty("hot")]
        public int Hot { get; set; }
    }

    public static class ModHelper
    {
        public static Version GetVersion(this Mod mod) => new Version(mod.Version.Substring(1));

        public static Version GetModLoaderVersion(this Mod mod) => new Version(mod.ModLoaderVersion.Substring(12));

        public static DateTime GetUpdateTimestamp(this Mod mod) => DateTime.Parse(mod.UpdateTimeStamp);
    }
}