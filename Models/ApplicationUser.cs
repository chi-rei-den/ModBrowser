using Microsoft.AspNetCore.Identity;

namespace Chireiden.ModBrowser.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public string AuthorName { get; set; }
    }
}
