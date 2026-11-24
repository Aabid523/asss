using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.Models
{
    public class Specialty
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Specialty name is required.")]
        [StringLength(100)]
        [Display(Name = "Specialty Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
