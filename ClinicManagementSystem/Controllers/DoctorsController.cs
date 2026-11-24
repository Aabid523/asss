using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Doctors
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Doctors.Include(d => d.Specialty).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => d.FullName.Contains(search) || d.Email.Contains(search));
                ViewData["CurrentSearch"] = search;
            }

            return View(await query.OrderBy(d => d.FullName).ToListAsync());
        }

        // GET: Doctors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Doctors
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null) return NotFound();

            return View(doctor);
        }

        // GET: Doctors/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["SpecialtyId"] = new SelectList(_context.Specialties.OrderBy(s => s.Name), "Id", "Name");
            return View();
        }

        // POST: Doctors/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,PhoneNumber,Email,SpecialtyId")] Doctor doctor)
        {
            if (await _context.Doctors.AnyAsync(d => d.Email == doctor.Email))
            {
                ModelState.AddModelError(nameof(doctor.Email), "A doctor with this email already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["SpecialtyId"] = new SelectList(_context.Specialties.OrderBy(s => s.Name), "Id", "Name", doctor.SpecialtyId);
                return View(doctor);
            }

            _context.Add(doctor);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Doctor created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Doctors/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            ViewData["SpecialtyId"] = new SelectList(_context.Specialties.OrderBy(s => s.Name), "Id", "Name", doctor.SpecialtyId);
            return View(doctor);
        }

        // POST: Doctors/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,PhoneNumber,Email,SpecialtyId")] Doctor doctor)
        {
            if (id != doctor.Id) return NotFound();

            if (await _context.Doctors.AnyAsync(d => d.Email == doctor.Email && d.Id != id))
            {
                ModelState.AddModelError(nameof(doctor.Email), "A doctor with this email already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["SpecialtyId"] = new SelectList(_context.Specialties.OrderBy(s => s.Name), "Id", "Name", doctor.SpecialtyId);
                return View(doctor);
            }

            try
            {
                _context.Update(doctor);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Doctor updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Doctors.AnyAsync(e => e.Id == doctor.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Doctors/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Doctors
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null) return NotFound();

            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasAppointments = await _context.Appointments.AnyAsync(a => a.DoctorId == id);
            if (hasAppointments)
            {
                TempData["ErrorMessage"] = "Cannot delete a doctor who has existing appointments or visits.";
                return RedirectToAction(nameof(Index));
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Doctor deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
