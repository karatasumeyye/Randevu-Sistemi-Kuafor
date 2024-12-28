using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Randevu_Sistemi_Kuafor.Models;
using System.Linq;
using System.Security.Claims;

namespace Randevu_Sistemi_Kuafor.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly SalonDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(SalonDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // İşlemleri döndür
        public IActionResult GetServices()
        {
            var services = _context.Services.ToList();
            return View(services);
        }

        // İşleme göre çalışanları döndür
        public JsonResult GetEmployeesByService(int serviceId)
        {
            var employees = _context.EmployeeServices
                .Where(es => es.ServiceId == serviceId)
                .Select(es => new
                {
                    es.Employee.EmployeeId,
                    es.Employee.User.Name
                })
                .ToList();

            return Json(employees);
        }

        // Müsait saatleri döndür
        public JsonResult GetAvailableTimes(int serviceId, int employeeId, string date)
        {
            var service = _context.Services.Find(serviceId);
            if (service == null)
                return Json(new { error = "Geçersiz servis." });

            var employee = _context.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);
            if (employee == null)
                return Json(new { error = "Geçersiz çalışan." });

            if (!DateTime.TryParse(date, out DateTime selectedDate))
                return Json(new { error = "Geçersiz tarih formatı." });

            var workingHoursStart = new TimeSpan(9, 0, 0);
            var workingHoursEnd = new TimeSpan(18, 0, 0);


          


            var appointments = _context.Appointments
                 .Include(a => a.Service) // Service tablosunu dahil ediyoruz
                 .Include(a=>a.User)
                .Where(a => a.EmployeeId == employeeId
                            && a.AppointmentDate.Date == selectedDate.Date.ToUniversalTime()
                            && a.Status == AppointmentStatus.Confirmed)
                .Select(a => new { a.AppointmentDate, a.Service.Duration })
                .ToList();

            var availableTimes = new List<string>();

            for (var time = workingHoursStart; time < workingHoursEnd; time = time.Add(TimeSpan.FromMinutes(30)))
            {
                bool isAvailable = !appointments.Any(a =>
                {
                    var appointmentEnd = a.AppointmentDate.TimeOfDay.Add(TimeSpan.FromMinutes(a.Duration));
                    return a.AppointmentDate.TimeOfDay < time.Add(TimeSpan.FromMinutes(30)) &&
                           appointmentEnd > time;
                });

                if (isAvailable)
                    availableTimes.Add(time.ToString(@"hh\:mm"));
            }

            return Json(new
            {
                availableTimes,
                serviceName = service.ServiceName,
               // employeeName = employee.User.Name,
                date = selectedDate.ToString("yyyy-MM-dd")
            });
        }

        // Randevuyu doğrula
        public JsonResult ConfirmAppointment(int serviceId, int employeeId, string date, string time)
        {
            var service = _context.Services.Find(serviceId);
            var employee = _context.Employees
                .Include(e => e.User)
                .FirstOrDefault(e => e.EmployeeId == employeeId);

            if (service == null || employee == null)
                return Json(new { success = false, error = "Geçersiz servis veya çalışan." });

            if (!DateTime.TryParse($"{date} {time}", out DateTime appointmentDateTime))
                return Json(new { success = false, error = "Geçersiz tarih veya saat." });

            return Json(new
            {
                success = true,
                serviceName = service.ServiceName,
                employeeName = employee.User.Name,
                appointmentTime = appointmentDateTime.ToString("yyyy-MM-dd HH:mm"),
                serviceCost = service.Price
            });
        }

        // Randevuyu kaydet
        [HttpPost]
        public async Task<JsonResult> SaveAppointment(int serviceId, int employeeId, string date, string time)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, error = "Geçersiz kullanıcı." });
            }

            var service = _context.Services.Find(serviceId);
            var employee = _context.Employees.Find(employeeId);

            if (service == null || employee == null)
            {
                return Json(new { success = false, error = "Geçersiz servis veya çalışan seçimi." });
            }

            if (!DateTime.TryParse(date + " " + time, out DateTime appointmentDateTime))
            {
                return Json(new { success = false, error = "Geçersiz tarih veya saat formatı." });
            }

            // Randevuyu oluştur
            var appointment = new Appointment
            {
                ServiceId = serviceId,
                EmployeeId = employeeId,
                Price = service.Price,
                AppointmentDate = appointmentDateTime.ToUniversalTime(), // Randevu tarihi ve saati
                Status = AppointmentStatus.Pending, // Başlangıç durumu
                UserId = userId // Kullanıcıyı ilişkilendir
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


























        // Kullanıcının randevularını görüntüle
        public async Task<IActionResult> UserAppointments()
        {
            var userId = _userManager.GetUserId(User);

            var appointments = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Employee)
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return View(appointments);
        }

        // Randevuyu iptal et
        [HttpPost]
        [Authorize]
        public IActionResult DeleteConfirmed(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null)
                return NotFound();

            if (appointment.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized();

            appointment.Status = AppointmentStatus.Cancelled;
            _context.SaveChanges();

            return RedirectToAction(nameof(UserAppointments));
        }
    }
}
