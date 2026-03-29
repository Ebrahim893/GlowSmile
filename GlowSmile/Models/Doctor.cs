using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlowSmile.Models
{
    public class Doctor
    {
        public int ID { get; set; }
        [Required]
        public string FullName { get; set; }
        public string Bio { get; set; }
        public string ImagePath { get; set; }
        public string Shift { get; set; } // (صباحي، ظهيرة، مسائي)
        public int MaxPatientsPerShift { get; set; } = 5;

        public int? ServiceID { get; set; }
        [ForeignKey("ServiceID")]
        public Service Service { get; set; }

        // العلاقة: الدكتور الواحد ليه حجوزات كتير
        public ICollection<Appointment> Appointments { get; set; }
    }
}