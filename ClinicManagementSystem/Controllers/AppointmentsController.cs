using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Appointments  (renders the FullCalendar view)
        public IActionResult Index() => View();

        // GET: Appointments/GetEvents  (JSON feed consumed by FullCalendar.js)
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToListAsync();

            var events = appointments.Select(a => new CalendarEventViewModel
            {
                id = a.Id,
                title = $"{a.Patient?.FullName} - Dr. {a.Doctor?.FullName}",
                start = a.AppointmentDate.ToString("s"),
                end = a.AppointmentDate.AddMinutes(a.DurationMinutes).ToString("s"),
                url = Url.Action(nameof(Details), new { id = a.Id }) ?? "#",
                backgroundColor = a.Status switch
                {
                    AppointmentStatus.Completed => "#3F8F5F",
                    AppointmentStatus.Cancelled => "#C0392B",
                    _ => "#0F6E66"
                },
                borderColor = a.Status switch
                {
                    AppointmentStatus.Completed => "#3F8F5F",
                    AppointmentStatus.Cancelled => "#C0392B",
                    _ => "#0F6E66"
                }
            });

            return Json(events);
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor).ThenInclude(d => d!.Specialty)
                .Include(a => a.Visit)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // GET: Appointments/Create
        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Create(DateTime? date)
        {
            PopulateDropdowns();
            var model = new Appointment
            {
                AppointmentDate = date ?? DateTime.Today.AddHours(DateTime.Now.Hour + 1)
            };
            return View(model);
        }

        // POST: Appointments/Create
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PatientId,DoctorId,AppointmentDate,DurationMinutes,Status,Notes")] Appointment appointment)
        {
            await ValidateNoConflict(appointment);

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(appointment.PatientId, appointment.DoctorId);
                return View(appointment);
            }

            _context.Add(appointment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Appointment booked successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Appointments/Edit/5
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            PopulateDropdowns(appointment.PatientId, appointment.DoctorId);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,DoctorId,AppointmentDate,DurationMinutes,Status,Notes")] Appointment appointment)
        {
            if (id != appointment.Id) return NotFound();

            await ValidateNoConflict(appointment);

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(appointment.PatientId, appointment.DoctorId);
                return View(appointment);
            }

            try
            {
                _context.Update(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Appointment updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Appointments.AnyAsync(e => e.Id == appointment.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Appointments/Delete/5
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Appointment deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ---- Helpers ----

        private void PopulateDropdowns(int? selectedPatientId = null, int? selectedDoctorId = null)
        {
            ViewData["PatientId"] = new SelectList(_context.Patients.OrderBy(p => p.FullName), "Id", "FullName", selectedPatientId);
            ViewData["DoctorId"] = new SelectList(_context.Doctors.OrderBy(d => d.FullName), "Id", "FullName", selectedDoctorId);
        }

        // Prevents double-booking: a doctor cannot have two overlapping, non-cancelled appointments.
        private async Task ValidateNoConflict(Appointment appointment)
        {
            if (appointment.DoctorId == 0) return;

            var newStart = appointment.AppointmentDate;
            var newEnd = appointment.AppointmentDate.AddMinutes(appointment.DurationMinutes);

            var conflict = await _context.Appointments
                .Where(a => a.Id != appointment.Id
                    && a.DoctorId == appointment.DoctorId
                    && a.Status != AppointmentStatus.Cancelled)
                .AnyAsync(a => newStart < a.AppointmentDate.AddMinutes(a.DurationMinutes)
                    && a.AppointmentDate < newEnd);

            if (conflict)
            {
                ModelState.AddModelError(string.Empty, "This doctor already has another appointment that overlaps with the selected time slot.");
            }
        }
    }
}
