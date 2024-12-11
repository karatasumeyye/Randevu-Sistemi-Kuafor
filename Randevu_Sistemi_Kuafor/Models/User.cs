using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Randevu_Sistemi_Kuafor.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(50)]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        // İlişkiler
        public ICollection<Appointment>? Appointments{ get; set; }
    }
}
