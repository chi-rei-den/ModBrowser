using Chireiden.ModBrowser.Data;
using Chireiden.ModBrowser.Models;
using Chireiden.ModBrowser.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using FileIO = System.IO.File;

namespace Chireiden.ModBrowser.Controllers
{
    public class LocalizerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LocalizerController> _logger;

        public LocalizerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<LocalizerController> logger)
        {
            this._context = context;
            this._userManager = userManager;
            this._logger = logger;
        }

        // GET: LocalizerPackages
        public async Task<IActionResult> Index(string? mod)
        {
            var applicationDbContext = this._context.Package.Include(l => l.Mod);
            IEnumerable<LocalizerPackage> result = await applicationDbContext.ToListAsync();
            if (!string.IsNullOrWhiteSpace(mod))
            {
                result = result.Where(p => p.ModName == mod);
            }
            return this.View(result);
        }

        // GET: LocalizerPackages
        public async Task<IActionResult> List()
        {
            var applicationDbContext = this._context.Package
                .Include(l => l.Mod)
                .Include(l => l.Uploader);
            return this.Content(JsonConvert.SerializeObject(await applicationDbContext.ToListAsync()), MediaTypeNames.Application.Json);
        }

        public async Task<IActionResult> Download(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var localizerPackage = await this._context.Package
                .FirstOrDefaultAsync(m => m.Id == id);

            if (localizerPackage == null || !FileIO.Exists(localizerPackage.FilePath()))
            {
                return this.NotFound();
            }
            return this.PhysicalFile(localizerPackage.FilePath(), MediaTypeNames.Application.Octet, $"{localizerPackage.Name}_{localizerPackage.Version}.locpack");
        }

        // GET: LocalizerPackages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var localizerPackage = await this._context.Package
                .Include(l => l.Mod)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (localizerPackage == null)
            {
                return this.NotFound();
            }

            return this.View(localizerPackage);
        }

        [Authorize]
        public IActionResult Create()
        {
            return this.View(new LocalizerPackageVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(LocalizerPackageVM mod)
        {
            if (this.ModelState.IsValid)
            {
                var length = (int)mod.File.Length;
                var buffer = new byte[length];
                mod.File.OpenReadStream().Read(buffer, 0, length);

                var entry = new LocalizerPackage();
                try
                {
                    PackageFile.FromFile(buffer).CopyTo(entry);
                }
                catch (Exception)
                {
                    throw;
                }

                var user = await this._userManager.GetUserAsync(this.User);
                entry.Uploader = user;
                entry.UploaderId = user.Id;

                this._logger.LogInformation($"User {user.UserName} ({user.AuthorName}) Create {entry.Name}");

                entry.CreateTimeStamp = entry.UpdateTimeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss");

                this._context.Add(entry);
                await this._context.SaveChangesAsync();

                var filename = entry.FilePath();
                if (FileIO.Exists(filename))
                {
                    FileIO.Delete(filename);
                }

                FileIO.WriteAllBytes(filename, buffer);
                return this.RedirectToAction(nameof(Details), new { id = entry.Id });
            }
            return this.View(mod);
        }

        // GET: LocalizerPackages/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var localizerPackage = await this._context.Package
                .Include(l => l.Mod)
                .Include(l => l.Uploader)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (localizerPackage == null)
            {
                return this.NotFound();
            }

            var user = await this._userManager.GetUserAsync(this.User);
            if (localizerPackage.Uploader.Id != user.Id)
            {
                return this.Forbid();
            }

            return this.View(localizerPackage);
        }

        // POST: LocalizerPackages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var localizerPackage = await this._context.Package
                .Include(l => l.Mod)
                .Include(l => l.Uploader)
                .FirstOrDefaultAsync(m => m.Id == id);

            var user = await this._userManager.GetUserAsync(this.User);
            if (localizerPackage.Uploader.Id != user.Id)
            {
                return this.Forbid();
            }

            this._context.Package.Remove(localizerPackage);
            await this._context.SaveChangesAsync();
            return this.RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var localizerPackage = await this._context.Package
                .Include(l => l.Mod)
                .Include(l => l.Uploader)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (localizerPackage == null)
            {
                return this.NotFound();
            }

            if (localizerPackage.Uploader.Id != (await this._userManager.GetUserAsync(this.User)).Id)
            {
                return this.Forbid();
            }

            return this.View(new LocalizerPackageVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int? id, LocalizerPackageVM mod)
        {
            var length = (int)mod.File.Length;
            var buffer = new byte[length];
            mod.File.OpenReadStream().Read(buffer, 0, length);

            if (this.ModelState.IsValid)
            {
                var existing = await this._context.Package
                    .Include(l => l.Mod)
                    .Include(l => l.Uploader)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (existing == null)
                {
                    return this.NotFound();
                }

                var user = await this._userManager.GetUserAsync(this.User);

                if (existing.Uploader.Id != user.Id)
                {
                    return this.Forbid();
                }

                try
                {
                    PackageFile.FromFile(buffer).CopyTo(existing);
                }
                catch (Exception)
                {
                    throw;
                }

                existing.UpdateTimeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss");

                this._logger.LogInformation($"User {user.UserName} ({user.AuthorName}) Update {existing.Name}");
                try
                {
                    var filename = existing.FilePath();
                    if (FileIO.Exists(filename))
                    {
                        FileIO.Delete(filename);
                    }

                    FileIO.WriteAllBytes(filename, buffer);

                    this._context.Update(existing);
                    await this._context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return this.RedirectToAction(nameof(Details), new { id = existing.Id });
            }
            return this.View(mod);
        }

        private bool LocalizerPackageExists(int id)
        {
            return this._context.Package.Any(e => e.Id == id);
        }
    }
}
