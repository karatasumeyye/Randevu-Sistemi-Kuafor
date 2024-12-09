using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Randevu_Sistemi_Kuafor.Models
{
    public class EmployeeService
    {
        [Key]
        public int EmpServiceId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        // İlişkiler
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        [ForeignKey("ServiceId")]
        public Service Service { get; set; }
    }
}
