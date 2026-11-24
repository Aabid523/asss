namespace ClinicManagementSystem.ViewModels
{
    // Matches the JSON shape FullCalendar.js expects from an events feed.
    public class CalendarEventViewModel
    {
        public int id { get; set; }
        public string title { get; set; } = string.Empty;
        public string start { get; set; } = string.Empty;
        public string end { get; set; } = string.Empty;
        public string url { get; set; } = string.Empty;
        public string backgroundColor { get; set; } = string.Empty;
        public string borderColor { get; set; } = string.Empty;
    }
}
