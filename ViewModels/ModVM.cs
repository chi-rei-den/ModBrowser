﻿using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Chireiden.ModBrowser.ViewModels
{
    public class ModVM
    {
        [Display(Name = "File", ResourceType = typeof(Resources.Localization))]
        public IFormFile File { set; get; }
    }
}
