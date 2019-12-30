using Microsoft.AspNetCore.Http;
using Chireiden.ModBrowser.Data;
using Chireiden.ModBrowser.ModLoader;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chireiden.ModBrowser.ViewModels
{
    public class ModVM : IValidatableObject
    {
        [Key, Required, DisplayName("Internal Name"), RegularExpression("^[!\\-_a-zA-Z0-9]+$")]
        public string Name { get; set; }

        [DisplayName("Name")]
        public string DisplayName { get; set; }

        [Required]
        public string Version { get; set; }

        [Required]
        public string Author { get; set; }

        [NotMapped]
        public string[] Authors => this.Author?.Split(", ");

        public string Description { get; set; }

        [DisplayName("tModLoader Version")]
        public string ModLoaderVersion { get; set; }

        [DisplayName("Referencing Mod")]
        public string ModReference { get; set; }

        [NotMapped]
        public string[] ModReferences => this.ModReference?.Split(", ");

        public string Homepage { get; set; }

        [DisplayName("Icon URL")]
        public string IconURL { get; set; }

        public ModSide ModSide { get; set; }

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
            if (!string.IsNullOrWhiteSpace(this.ModReference))
            {
                var dbContext = (ApplicationDbContext)validationContext.GetService(typeof(ApplicationDbContext));
                foreach (var item in this.ModReferences)
                {
                    if (dbContext.Mod.Find(item) == null)
                    {
                        yield return new ValidationResult($"Referenced mod {item} not exist", new[] { nameof(this.ModReference) });
                    }
                }
            }
        }
    }
}
