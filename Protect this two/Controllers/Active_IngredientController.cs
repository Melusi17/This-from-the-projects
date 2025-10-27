using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IbhayiPharmacy.Data;
using IbhayiPharmacy.Models;

namespace IbhayiPharmacy.Controllers
{
    public class Active_IngredientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public Active_IngredientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Active_Ingredient
        public async Task<IActionResult> Index()
        {
            return View(await _context.Active_Ingredients.ToListAsync());
        }

        // GET: Active_Ingredient/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var active_Ingredient = await _context.Active_Ingredients
                .FirstOrDefaultAsync(m => m.Active_IngredientID == id);
            if (active_Ingredient == null)
            {
                return NotFound();
            }

            return View(active_Ingredient);
        }

        // GET: Active_Ingredient/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Active_Ingredient/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Active_IngredientID,Name")] Active_Ingredient active_Ingredient)
        {
            if (ModelState.IsValid)
            {
                _context.Add(active_Ingredient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(active_Ingredient);
        }

        // GET: Active_Ingredient/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var active_Ingredient = await _context.Active_Ingredients.FindAsync(id);
            if (active_Ingredient == null)
            {
                return NotFound();
            }
            return View(active_Ingredient);
        }

        // POST: Active_Ingredient/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Active_IngredientID,Name")] Active_Ingredient active_Ingredient)
        {
            if (id != active_Ingredient.Active_IngredientID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(active_Ingredient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Active_IngredientExists(active_Ingredient.Active_IngredientID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(active_Ingredient);
        }

        // GET: Active_Ingredient/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var active_Ingredient = await _context.Active_Ingredients
                .FirstOrDefaultAsync(m => m.Active_IngredientID == id);
            if (active_Ingredient == null)
            {
                return NotFound();
            }

            return View(active_Ingredient);
        }

        // POST: Active_Ingredient/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var active_Ingredient = await _context.Active_Ingredients.FindAsync(id);
            if (active_Ingredient != null)
            {
                _context.Active_Ingredients.Remove(active_Ingredient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Active_IngredientExists(int id)
        {
            return _context.Active_Ingredients.Any(e => e.Active_IngredientID == id);
        }
    }
}
