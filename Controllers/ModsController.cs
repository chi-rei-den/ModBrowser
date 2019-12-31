using Chireiden.ModBrowser.Data;
using Chireiden.ModBrowser.Models;
using Chireiden.ModBrowser.ModLoader;
using Chireiden.ModBrowser.Services;
using Chireiden.ModBrowser.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileIO = System.IO.File;

namespace Chireiden.ModBrowser.Controllers
{
    public class ModsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ModsController> _logger;

        public ModsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<ModsController> logger)
        {
            this._context = context;
            this._userManager = userManager;
            this._logger = logger;
        }

        public IActionResult Index(string by, string order, string search, string filter, int? page)
        {
            if (!string.IsNullOrWhiteSpace(search) || page == null)
            {
                page = 1;
            }

            this.ViewData["by"] = by;
            this.ViewData["search"] = search ??= filter;
            this.ViewData["page"] = page;

            IEnumerable<Mod> result = this._context.Mod;

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                result = result.Where(r => r.Name.ToLower().Contains(search));
            }

            result = by switch
            {
                "v" => result.OrderBy(r => r.Version),
                "m" => result.OrderBy(r => r.GetModLoaderVersion()),
                "n" => result.OrderByDescending(r => r.Name),
                "a" => result.OrderByDescending(r => r.Author),
                "h" => result.OrderBy(r => r.Hot),
                "s" => result.OrderBy(r => r.Size),
                "u" => result.OrderBy(r => r.GetUpdateTimestamp()),
                "d" => result.OrderBy(r => r.Downloads),
                _ => result.OrderByDescending(r => r.Downloads),
            };

            if (by == order)
            {
                this.ViewData["order"] = "";
            }
            else
            {
                result = result.Reverse();
                this.ViewData["order"] = by;
            }

            return this.View(result.ToPaginated(page.Value, 20).ToList());
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var mod = await this._context.Mod
                .FirstOrDefaultAsync(m => m.Name == id);
            return mod == null ? this.NotFound() : (IActionResult)this.View(mod);
        }

        [Authorize]
        public IActionResult Create()
        {
            return this.View(new ModVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(ModVM mod)
        {
            if (this.ModelState.IsValid)
            {
                var length = (int)mod.File.Length;
                var buffer = new byte[length];
                mod.File.OpenReadStream().Read(buffer, 0, length);

                var entry = new Mod();
                if (!entry.ExtractInfo(buffer, true))
                {
                    return this.UnprocessableEntity();
                }

                if (this._context.Mod.Find(entry.Name) != null)
                {
                    return this.Conflict();
                }

                var user = await this._userManager.GetUserAsync(this.User);
                if (!entry.Author.Split(", ").Contains(user.AuthorName))
                {
                    entry.Author += ", " + user.AuthorName;
                }

                this._logger.LogInformation($"User {user.UserName} ({user.AuthorName}) Create {entry.DisplayName} ({entry.Name})");

                var filename = entry.FilePath();
                if (FileIO.Exists(filename))
                {
                    FileIO.Delete(filename);
                }

                FileIO.WriteAllBytes(filename, buffer);

                entry.UpdateTimeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss");

                this._context.Add(entry);
                await this._context.SaveChangesAsync();
                return this.RedirectToAction(nameof(Details), new { id = entry.Name });
            }
            return this.View(mod);
        }

        [Authorize]
        public async Task<IActionResult> Sync(string id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var mod = await this._context.Mod.FindAsync(id);
            if (mod == null)
            {
                return this.NotFound();
            }

            SyncService.UpdateRequested.Enqueue(id);

            return this.RedirectToAction("Details", new { id });
        }

        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var mod = await this._context.Mod.FindAsync(id);
            if (mod == null)
            {
                return this.NotFound();
            }

            return this.View(new ModVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(string id, ModVM mod)
        {
            var length = (int)mod.File.Length;
            var buffer = new byte[length];
            mod.File.OpenReadStream().Read(buffer, 0, length);

            var entry = new Mod();

            if (!entry.ExtractInfo(buffer))
            {
                return this.UnprocessableEntity();
            }

            if (id != entry.Name)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                var existing = this._context.Mod.Find(id);
                if (existing == null)
                {
                    return this.NotFound();
                }

                var user = await this._userManager.GetUserAsync(this.User);
                if (!existing.Author.Split(", ").Contains(user.AuthorName))
                {
                    return this.Forbid();
                }

                existing.ExtractInfo(buffer, true);

                entry.UpdateTimeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss");

                this._logger.LogInformation($"User {user.UserName} ({user.AuthorName}) Update {existing.DisplayName} ({existing.Name})");
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
                return this.RedirectToAction(nameof(Details), new { id = entry.Name });
            }
            return this.View(mod);
        }

        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var mod = await this._context.Mod
                .FirstOrDefaultAsync(m => m.Name == id);
            if (mod == null)
            {
                return this.NotFound();
            }

            return this.View(mod);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var mod = await this._context.Mod.FindAsync(id);
            var user = await this._userManager.GetUserAsync(this.User);
            if (!mod.Author.Split(", ").Contains(user.AuthorName))
            {
                return this.Forbid();
            }

            this._logger.LogInformation($"User {user.UserName} ({user.AuthorName}) Delete {mod.DisplayName} ({mod.Name})");
            this._context.Mod.Remove(mod);
            await this._context.SaveChangesAsync();
            return this.RedirectToAction(nameof(Index));
        }
    }
}
