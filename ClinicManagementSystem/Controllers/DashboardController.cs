using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var sevenDaysAgo = today.AddDays(-6);

            var vm = new DashboardViewModel
            {
                TotalPatients = await _context.Patients.CountAsync(),
                TotalDoctors = await _context.Doctors.CountAsync(),
                TotalVisits = await _context.Visits.CountAsync(),
                TodaysAppointments = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == today)
            };

            // Appointments per day for the last 7 days
            var appointmentsLast7Days = await _context.Appointments
                .Where(a => a.AppointmentDate.Date >= sevenDaysAgo && a.AppointmentDate.Date <= today)
                .ToListAsync();

            for (var day = sevenDaysAgo; day <= today; day = day.AddDays(1))
            {
                vm.Last7DaysLabels.Add(day.ToString("MMM dd"));
                vm.Last7DaysCounts.Add(appointmentsLast7Days.Count(a => a.AppointmentDate.Date == day));
            }

            // Appointment breakdown by status (all-time)
            var statusGroups = await _context.Appointments
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (AppointmentStatus status in Enum.GetValues(typeof(AppointmentStatus)))
            {
                vm.StatusLabels.Add(status.ToString());
                vm.StatusCounts.Add(statusGroups.FirstOrDefault(s => s.Status == status)?.Count ?? 0);
            }

            // Upcoming appointments (next 5)
            vm.UpcomingAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.AppointmentDate >= DateTime.Now && a.Status == AppointmentStatus.Scheduled)
                .OrderBy(a => a.AppointmentDate)
                .Take(5)
                .ToListAsync();

            return View(vm);
        }
    }
}
