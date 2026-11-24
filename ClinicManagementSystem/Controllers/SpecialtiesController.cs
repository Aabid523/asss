using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class SpecialtiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SpecialtiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Specialties
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Specialties.Include(s => s.Doctors).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.Name.Contains(search));
                ViewData["CurrentSearch"] = search;
            }

            return View(await query.OrderBy(s => s.Name).ToListAsync());
        }

        // GET: Specialties/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var specialty = await _context.Specialties
                .Include(s => s.Doctors)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (specialty == null) return NotFound();

            return View(specialty);
        }

        // GET: Specialties/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        // POST: Specialties/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Specialty specialty)
        {
            if (await _context.Specialties.AnyAsync(s => s.Name == specialty.Name))
            {
                ModelState.AddModelError(nameof(specialty.Name), "A specialty with this name already exists.");
            }

            if (!ModelState.IsValid) return View(specialty);

            _context.Add(specialty);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Specialty created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Specialties/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty == null) return NotFound();

            return View(specialty);
        }

        // POST: Specialties/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Specialty specialty)
        {
            if (id != specialty.Id) return NotFound();

            if (await _context.Specialties.AnyAsync(s => s.Name == specialty.Name && s.Id != id))
            {
                ModelState.AddModelError(nameof(specialty.Name), "A specialty with this name already exists.");
            }

            if (!ModelState.IsValid) return View(specialty);

            try
            {
                _context.Update(specialty);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Specialty updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Specialties.AnyAsync(e => e.Id == specialty.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Specialties/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var specialty = await _context.Specialties
                .Include(s => s.Doctors)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (specialty == null) return NotFound();

            return View(specialty);
        }

        // POST: Specialties/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var specialty = await _context.Specialties.Include(s => s.Doctors).FirstOrDefaultAsync(s => s.Id == id);
            if (specialty == null) return RedirectToAction(nameof(Index));

            if (specialty.Doctors.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete a specialty that still has doctors assigned to it.";
                return RedirectToAction(nameof(Index));
            }

            _context.Specialties.Remove(specialty);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Specialty deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
