using System;
using System.ComponentModel.DataAnnotations;

namespace ModBrowser.Models
{
    public class Mod
    {
        public string DisplayName { get; set; }

        [Key]
        public string Name { get; set; }

        public string Version { get; set; }

        public string Author { get; set; }

        [DataType(DataType.Date)]
        public DateTime UpdateTimeStamp { get; set; }

        public string Description { get; set; }

        public string ModLoaderVersion { get; set; }

        public string ModReferences { get; set; }

        public string Homepage { get; set; }

        public string Icon { get; set; }

        public string ModSide { get; set; }
    }
}
