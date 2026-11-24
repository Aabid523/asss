using Microsoft.AspNetCore.Identity;

namespace ClinicManagementSystem.Models
{
    // Extends the default Identity user with a display name used across the UI.
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
