using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagementSystem.Models
{
    public class Visit
    {
        public int Id { get; set; }

        [Display(Name = "Related Appointment")]
        public int? AppointmentId { get; set; }

        [ForeignKey(nameof(AppointmentId))]
        public Appointment? Appointment { get; set; }

        [Required(ErrorMessage = "Please select a patient.")]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient? Patient { get; set; }

        [Required(ErrorMessage = "Please select a doctor.")]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public Doctor? Doctor { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Visit Date")]
        public DateTime VisitDate { get; set; } = DateTime.Today;

        [StringLength(300)]
        public string? Diagnosis { get; set; }

        [StringLength(300)]
        public string? Treatment { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
