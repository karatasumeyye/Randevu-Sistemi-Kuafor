using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Randevu_Sistemi_Kuafor.Models;

namespace Randevu_Sistemi_Kuafor.Controllers
{
    public class EmployeeController : Controller
    {

        private readonly SalonDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public EmployeeController(SalonDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //[Authorize(Roles = "Employee")] // Add authorization
        public async Task<IActionResult> GetAppointment()
        {
            var userId = _userManager.GetUserId(User);
            
            var employee = await _context.Employees.FirstOrDefaultAsync(e=>e.UserId ==userId);

            if (employee == null)
            {
                return Unauthorized();
            }

            var appointments = await _context.Appointments
           .Include(a => a.Service)
           .Include(a => a.User)
           .Include(a => a.Employee)
           .Where(a => a.EmployeeId == employee.EmployeeId) // Filter by employee ID
           .ToListAsync();

            return View(appointments);
        }


        [HttpPost]
        [Authorize(Roles = "Admin, Employee")]
        public JsonResult ApproveAppointment(int appointmentId)
        {
            var appointment = _context.Appointments.Find(appointmentId);

            if (appointment == null)
            {
                return Json(new { success = false, error = "Randevu bulunamadı." });
            }

            // Çalışanın sadece kendi randevusunu onaylamasını kontrol et
            var employeeId = // Giriş yapan çalışanın ID'sini alın
                int.Parse(User.Claims.FirstOrDefault(c => c.Type == "EmployeeId")?.Value ?? "0");

            //if (appointment.EmployeeId != employeeId)
            //{
            //    return Json(new { success = false, error = "Bu randevuyu onaylama yetkiniz yok." });
            //}

            // Durumu Confirmed olarak güncelle
            appointment.Status = AppointmentStatus.Confirmed;
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Employee")]
        public JsonResult CancelAppointment(int appointmentId)
        {
            var appointment = _context.Appointments.Find(appointmentId);

            if (appointment == null)
            {
                return Json(new { success = false, error = "Randevu bulunamadı." });
            }

            // Çalışanın sadece kendi randevusunu iptal etmesini kontrol et
            var employeeId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "EmployeeId")?.Value ?? "0");

            //// Admin rolü kontrolü ekleyin
            //var isAdmin = User.IsInRole("Admin");

            //if (!isAdmin && appointment.EmployeeId != employeeId)
            //{
            //    return Json(new { success = false, error = "Bu randevuyu iptal etme yetkiniz yok." });
            //}

            // Durumu Cancelled olarak güncelle
            appointment.Status = AppointmentStatus.Cancelled;
            _context.SaveChanges();

            return Json(new { success = true });
        }

    }
}
