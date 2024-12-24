using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Randevu_Sistemi_Kuafor.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        public string UserId { get; set; } // IdentityUser.Id ile ilişkilendirilecek

        [StringLength(50)]
        public string Specialty { get; set; }

        //Null olabilir, sonradan editleme yapılabilir
        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        // İlişkiler
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<EmployeeService> EmployeeServices { get; set; }
    }
}
