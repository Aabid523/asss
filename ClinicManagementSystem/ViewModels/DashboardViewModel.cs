namespace ClinicManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TodaysAppointments { get; set; }
        public int TotalVisits { get; set; }

        // Appointments booked per day, last 7 days (for a line/bar chart)
        public List<string> Last7DaysLabels { get; set; } = new();
        public List<int> Last7DaysCounts { get; set; } = new();

        // Appointment breakdown by status (for a pie/doughnut chart)
        public List<string> StatusLabels { get; set; } = new();
        public List<int> StatusCounts { get; set; } = new();

        public List<ClinicManagementSystem.Models.Appointment> UpcomingAppointments { get; set; } = new();
    }
}
