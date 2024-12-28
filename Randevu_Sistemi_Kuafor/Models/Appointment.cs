using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Randevu_Sistemi_Kuafor.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public string UserId { get; set; } // IdentityUser.Id ile ilişkilendirilecek

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; }

        // İlişkiler
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        [ForeignKey("ServiceId")]
        public Service Service { get; set; }
    }


    public enum AppointmentStatus
    {
        Pending,  //Askıda
        Confirmed,  //Onaylandı
        Completed,   // Tamamlanmış
        Cancelled    // İptal edildi
    }
}
