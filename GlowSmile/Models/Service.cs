using System.ComponentModel.DataAnnotations;

namespace GlowSmile.Models
{
    public class Service
    {
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string ImagePath { get; set; }

        // العلاقة: الخدمة الواحدة ليها دكاترة كتير
        public ICollection<Doctor> Doctors { get; set; }
    }
}