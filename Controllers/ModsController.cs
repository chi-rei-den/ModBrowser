using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModBrowser.Data;
using ModBrowser.Models;
using ModBrowser.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModBrowser.Controllers
{
    public class ModsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ModsController(ApplicationDbContext context)
        {
            this._context = context;
        }

        // GET: Mods
        public async Task<IActionResult> Index(string by, bool order, string search)
        {
            this.ViewData["Domain"] = $"{this.Request.Scheme}://{this.Request.Host}/";
            IEnumerable<Mod> result = this._context.Mod;

            if (!string.IsNullOrWhiteSpace(search))
            {
                result = result.Where(r => r.Name.Contains(search));
            }

            switch (by)
            {
                case "v":
                    result = result.OrderBy(r => r.Version);
                    break;
                case "m":
                    result = result.OrderBy(r => r.GetModLoaderVersion());
                    break;
                case "n":
                    result = result.OrderBy(r => r.Name);
                    break;
                case "u":
                    result = result.OrderBy(r => r.GetUpdateTimestamp());
                    break;
                case "a":
                    result = result.OrderBy(r => r.Author);
                    break;
                case "d":
                    result = result.OrderBy(r => r.Downloads);
                    break;
                case "h":
                    result = result.OrderBy(r => r.Hot);
                    break;
                default:
                    break;
            }

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
            if (mod == null)
            {
                return this.NotFound();
            }

            return this.View(mod);
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

                if (!mod.Author.Split(", ").Contains(this.User.FindFirstValue(ClaimTypes.Name)))
                {
                    mod.Author += ", " + this.User.FindFirstValue(ClaimTypes.Name);
                }

                var entry = new Mod();
                this._context.Entry(entry).CurrentValues.SetValues(mod);
                this._context.Add(entry);
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
            return this.View(mod);
        }

        // POST: Mods/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(string id, [Bind("DisplayName,Name,Version,Author,UpdateTimeStamp,Description,ModLoaderVersion,ModReferences,Homepage,Icon,ModSide")] Mod mod)
        {
            if (id != mod.Name)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                var existing = this._context.Mod.Find(id);
                if (!existing.Author.Split(", ").Contains(this.User.FindFirstValue(ClaimTypes.Name)))
                {
                    return this.Forbid();
                }

                try
                {
                    this._context.Update(mod);
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
            if (!mod.Author.Split(", ").Contains(this.User.FindFirstValue(ClaimTypes.Name)))
            {
                return this.Forbid();
            }

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
