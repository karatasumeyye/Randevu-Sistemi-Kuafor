using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Randevu_Sistemi_Kuafor.Models;

namespace Randevu_Sistemi_Kuafor.Models
{
    public class EmployeeLeave
    {
        [Key]
        public int LeaveId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DateTime LeaveStartDate { get; set; }

        [Required]
        public DateTime LeaveEndDate { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        // İlişkiler
        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }
    }


}
