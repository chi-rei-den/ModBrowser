using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ModBrowser.ViewModels
{
    public class ModVM : IValidatableObject
    {
        [Required, DisplayName("Internal Name")]
        public string Name { get; set; }

        [DisplayName("Name")]
        public string DisplayName { get; set; }

        [Required]
        public string Version { get; set; }

        [Required]
        public List<string> Author { get; set; } = new List<string>();

        public string Description { get; set; }

        public string ModLoaderVersion { get; set; }

        public List<string> ModReferences { get; set; } = new List<string>();

        public string Homepage { get; set; }

        public string IconURL { get; set; }

        public ModSide ModSide { get; set; } = ModSide.Both;

        public IFormFile File { set; get; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(this.ModLoaderVersion) && !System.Version.TryParse(this.ModLoaderVersion, out var _))
            {
                yield return new ValidationResult("Mod Loader version invalid", new[] { nameof(this.ModLoaderVersion) });
            }
            if (!string.IsNullOrWhiteSpace(this.Version) && !System.Version.TryParse(this.Version, out var _))
            {
                yield return new ValidationResult("Version invalid", new[] { nameof(this.Version) });
            }
        }
    }

    public enum ModSide
    {
        Both,
        Client,
        Server,
        NoSync
    }
}
