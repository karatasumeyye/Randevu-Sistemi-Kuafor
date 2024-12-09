using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Randevu_Sistemi_Kuafor.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        [StringLength(100)]
        public string ServiceName { get; set; }

        [Required]
        public int Duration { get; set; }  // Dakika cinsinden

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // İlişkiler
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<EmployeeService> EmployeeServices { get; set; }
    }
}
