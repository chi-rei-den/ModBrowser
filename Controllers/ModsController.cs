using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModBrowser.Data;
using ModBrowser.Models;
using ModBrowser.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileIO = System.IO.File;

namespace ModBrowser.Controllers
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

        // GET: Mods
        public IActionResult Index(string by, bool order, string search)
        {
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
                "n" => result.OrderBy(r => r.Name),
                "a" => result.OrderBy(r => r.Author),
                "d" => result.OrderBy(r => r.Downloads),
                "h" => result.OrderBy(r => r.Hot),
                _ => result.OrderBy(r => r.GetUpdateTimestamp()),
            };

            this.ViewData["Order"] = !order;
            if (Convert.ToBoolean(this.ViewData["Order"]))
            {
                result = result.Reverse();
            }

            return this.View(result.ToList());
        }

        // GET: Mods/Details/5
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

        // GET: Mods/Create
        [Authorize]
        public IActionResult Create()
        {
            return this.View(new ModVM());
        }

        // POST: Mods/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(ModVM mod)
        {
            if (this.ModelState.IsValid)
            {
                if (this._context.Mod.Find(mod.Name) != null)
                {
                    return this.Conflict();
                }

                var user = await this._userManager.GetUserAsync(this.User);
                if (!mod.Author.Split(", ").Contains(user.AuthorName))
                {
                    mod.Author += ", " + user.AuthorName;
                }

                this._logger.LogInformation($"User {user.UserName} ({user.AuthorName}) Create {mod.DisplayName} ({mod.Name})");
                var entry = new Mod();
                this._context.Entry(entry).CurrentValues.SetValues(mod);
                this._context.Add(entry);
                var filename = mod.FilePath();
                if (FileIO.Exists(filename))
                {
                    FileIO.Delete(filename);
                }
                mod.File.CopyTo(FileIO.OpenWrite(filename));
                await this._context.SaveChangesAsync();
                return this.RedirectToAction(nameof(Index));
            }
            return this.View(mod);
        }

        // GET: Mods/Edit/5
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

            var m = new ModVM();
            var track = this._context.Entry(m);
            track.CurrentValues.SetValues(mod);
            track.State = EntityState.Detached;
            return this.View(m);
        }

        // POST: Mods/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(string id, ModVM mod)
        {
            if (id != mod.Name)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                var existing = this._context.Mod.Find(id);
                var user = await this._userManager.GetUserAsync(this.User);
                if (!existing.Author.Split(", ").Contains(user.AuthorName))
                {
                    return this.Forbid();
                }

                this._context.Entry(existing).CurrentValues.SetValues(mod);
                this._logger.LogInformation($"User {user.UserName} ({user.AuthorName}) Update {mod.DisplayName} ({mod.Name})");
                try
                {
                    var filename = mod.FilePath();
                    if (FileIO.Exists(filename))
                    {
                        FileIO.Delete(filename);
                    }
                    mod.File.CopyTo(FileIO.OpenWrite(filename));
                    this._context.Update(existing);
                    await this._context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.ModExists(mod.Name))
                    {
                        return this.NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return this.RedirectToAction(nameof(Index));
            }
            return this.View(mod);
        }

        // GET: Mods/Delete/5
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

        // POST: Mods/Delete/5
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

        private bool ModExists(string id)
        {
            return this._context.Mod.Any(e => e.Name == id);
        }
    }
}
