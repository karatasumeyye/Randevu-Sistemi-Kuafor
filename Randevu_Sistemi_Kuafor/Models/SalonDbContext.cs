using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Randevu_Sistemi_Kuafor.Models
{
    public class SalonDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public SalonDbContext(DbContextOptions<SalonDbContext> options)
             : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<EmployeeLeave> EmployeeLeaves { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<EmployeeService> EmployeeServices { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host = localhost; Port = 5432; Database = Randevu_Sistemi_Kuafor_DB; Username = postgres; Password =1234");

        }
    }
}
