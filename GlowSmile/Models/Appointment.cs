using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlowSmile.Models
{
    public class Appointment
    {
        public int ID { get; set; }

        [Required]
        public string PatientName { get; set; }

        [Required]
        public string PatientPhone { get; set; }

        [Required]
        [EmailAddress]
        public string PatientEmail { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public string ShiftTime { get; set; }

        public string? Message { get; set; }

        public bool IsConfirmed { get; set; } = false;

        // الحقل المخصص لوقت الأدمن
        public string? AssignedTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? DoctorID { get; set; }
        [ForeignKey("DoctorID")]
        public Doctor? Doctor { get; set; }
    }
}