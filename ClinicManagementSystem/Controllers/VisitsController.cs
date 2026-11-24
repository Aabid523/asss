using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Doctor")]
    public class VisitsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VisitsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Visits
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Visits
                .Include(v => v.Patient)
                .Include(v => v.Doctor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(v => v.Patient!.FullName.Contains(search) || v.Doctor!.FullName.Contains(search));
                ViewData["CurrentSearch"] = search;
            }

            return View(await query.OrderByDescending(v => v.VisitDate).ToListAsync());
        }

        // GET: Visits/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var visit = await _context.Visits
                .Include(v => v.Patient)
                .Include(v => v.Doctor)
                .Include(v => v.Appointment)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visit == null) return NotFound();

            return View(visit);
        }

        // GET: Visits/Create
        public IActionResult Create(int? appointmentId)
        {
            PopulateDropdowns();

            var model = new Visit();
            if (appointmentId.HasValue)
            {
                var appointment = _context.Appointments.Find(appointmentId.Value);
                if (appointment != null)
                {
                    model.AppointmentId = appointment.Id;
                    model.PatientId = appointment.PatientId;
                    model.DoctorId = appointment.DoctorId;
                    model.VisitDate = appointment.AppointmentDate.Date;
                }
            }

            return View(model);
        }

        // POST: Visits/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppointmentId,PatientId,DoctorId,VisitDate,Diagnosis,Treatment,Notes")] Visit visit)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(visit.PatientId, visit.DoctorId);
                return View(visit);
            }

            _context.Add(visit);

            // Mark the linked appointment as completed, if any.
            if (visit.AppointmentId.HasValue)
            {
                var appointment = await _context.Appointments.FindAsync(visit.AppointmentId.Value);
                if (appointment != null)
                {
                    appointment.Status = AppointmentStatus.Completed;
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Visit record saved successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Visits/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var visit = await _context.Visits.FindAsync(id);
            if (visit == null) return NotFound();

            PopulateDropdowns(visit.PatientId, visit.DoctorId);
            return View(visit);
        }

        // POST: Visits/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppointmentId,PatientId,DoctorId,VisitDate,Diagnosis,Treatment,Notes")] Visit visit)
        {
            if (id != visit.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(visit.PatientId, visit.DoctorId);
                return View(visit);
            }

            try
            {
                _context.Update(visit);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Visit record updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Visits.AnyAsync(e => e.Id == visit.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Visits/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var visit = await _context.Visits
                .Include(v => v.Patient)
                .Include(v => v.Doctor)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visit == null) return NotFound();

            return View(visit);
        }

        // POST: Visits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var visit = await _context.Visits.FindAsync(id);
            if (visit != null)
            {
                _context.Visits.Remove(visit);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Visit record deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns(int? selectedPatientId = null, int? selectedDoctorId = null)
        {
            ViewData["PatientId"] = new SelectList(_context.Patients.OrderBy(p => p.FullName), "Id", "FullName", selectedPatientId);
            ViewData["DoctorId"] = new SelectList(_context.Doctors.OrderBy(d => d.FullName), "Id", "FullName", selectedDoctorId);
            ViewData["AppointmentId"] = new SelectList(
                _context.Appointments.OrderByDescending(a => a.AppointmentDate),
                "Id", "Id");
        }
    }
}
