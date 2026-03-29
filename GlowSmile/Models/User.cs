using System.ComponentModel.DataAnnotations;

namespace GlowSmile.Models
{
    public class User
    {
        public int ID { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public string Role { get; set; } // (Admin / Staff)
    }
}