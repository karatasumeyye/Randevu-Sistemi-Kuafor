﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Randevu_Sistemi_Kuafor.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        public int UserId { get; set; }

        [StringLength(50)]
        public string Specialty { get; set; }

        // İlişkiler
        [ForeignKey("UserId")]
        public User User { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<EmployeeService> EmployeeServices { get; set; }
    }
}
