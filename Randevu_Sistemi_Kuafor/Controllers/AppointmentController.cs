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

        // Tüm işlemleri döndür
        public IActionResult GetServices()
        {
            var services = _context.Services.ToList();
            return View(services);
        }

        // Seçilen işlemi yapabilen çalışanları döndür
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


        // Seçilen tarihteki müsait saatleri döndür
       public JsonResult GetAvailableTimes(int serviceId, int employeeId, string date)
{
    var service = _context.Services.Find(serviceId);
    if (service == null)
        return Json(new { error = "Geçersiz servis seçimi." });
    
    var employee = _context.Employees
        .Include(e => e.User)
        .FirstOrDefault(e => e.EmployeeId == employeeId);

    if (employee == null || employee.User == null)
    {
        return Json(new { error = "Geçersiz çalışan veya kullanıcı bulunamadı." });
    }

    if (!DateTime.TryParse(date, out DateTime selectedDate))
        return Json(new { error = "Geçersiz tarih formatı." });

            var appointmentDuration = TimeSpan.FromMinutes(service.Duration); // Servis süresi
            var workingHoursStart = new TimeSpan(9, 0, 0);
            var workingHoursEnd = new TimeSpan(18, 0, 0); // Çalışma saati
            var appointments = _context.Appointments
     .Where(a => a.EmployeeId == employeeId &&
                 a.AppointmentDate.Date == selectedDate.Date &&
                 a.Status == AppointmentStatus.Confirmed) // Onaylı randevuları al
     .Select(a => new {
         AppointmentDateUtc = a.AppointmentDate.Kind == DateTimeKind.Unspecified
             ? a.AppointmentDate.ToUniversalTime()  // UTC'ye dönüştür
             : a.AppointmentDate.ToUniversalTime(), // Zaten UTC ise de UTC'ye dönüştür
         a.DateTime,  // Bu bir TimeSpan olduğu için saat bilgisi
         a.AppointmentDate
     })
     .ToList();

            var availableTimes = new List<string>();

    for (var time = workingHoursStart; time < workingHoursEnd; time = time.Add(TimeSpan.FromMinutes(30))) // 30 dakikalık dilimler
    {
        bool isAvailable = true;

        // Mevcut randevuları kontrol et ve uygun saatin dolup dolmadığını kontrol et
        foreach (var appointment in appointments)
        {
            if (appointment.DateTime >= time && appointment.DateTime < time.Add(appointmentDuration))
            {
                isAvailable = false;
                break;
            }
        }

        if (isAvailable)
        {
            availableTimes.Add(time.ToString(@"hh\:mm"));
        }
    }

    return Json(new
    {
        availableTimes = availableTimes,
        serviceName = service.ServiceName,
        employeeName = employee.User.Name,
        date = selectedDate.ToString("yyyy-MM-dd")
    });
}


        public JsonResult ConfirmAppointment(int serviceId, int employeeId, string date, string time)
        {
            // İşlem ve çalışan bilgilerini al
            var service = _context.Services.Find(serviceId);
            var employee = _context.Employees
                  .Include(e => e.User) // User ilişkisini yükle
                  .FirstOrDefault(e => e.EmployeeId == employeeId);

            if (service == null || employee == null)
            {
                return Json(new { success = false, error = "Geçersiz servis veya çalışan seçimi." });
            }

            // Veritabanından çekilen servis fiyatını kontrol et
            if (service.Price == null)
            {
                return Json(new { success = false, error = "Servisin fiyatı belirtilmemiş." });
            }

            // User nesnesinin null olup olmadığını kontrol et
            if (employee.User == null)
            {
                return Json(new { success = false, error = "Çalışan için kullanıcı bilgisi bulunamadı." });
            }

            // Tarih ve saat bilgilerini düzenle
            if (!DateTime.TryParse(date + " " + time, out DateTime appointmentDateTime))
            {
                return Json(new { success = false, error = "Geçersiz tarih veya saat formatı." });
            }

 
            return Json(new
            {
                success = true,
                serviceName = service.ServiceName,
                employeeName = employee.User.Name,
                appointmentTime = appointmentDateTime.ToString("yyyy-MM-dd HH:mm"),
                serviceCost = service.Price // Servis ücretini ekledik
            });
        }




        public async Task<JsonResult> SaveAppointment(int serviceId, int employeeId, string date, string time)
        {
            var user = await _userManager.GetUserAsync(User); // Giriş yapmış kullanıcı
            var userId = user?.Id;

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, error = "Geçersiz kullanıcı." });
            }

            // İşlem ve çalışan bilgilerini al
            var service = _context.Services.Find(serviceId);
            var employee = _context.Employees.Find(employeeId);

            if (service == null || employee == null)
            {
                return Json(new { success = false, error = "Geçersiz servis veya çalışan seçimi." });
            }

            // Tarih ve saat bilgisini düzenle
            DateTime localDateTime = DateTime.Parse(date + " " + time);

            // Eğer DateTime türü Unspecified ise UTC'ye dönüştür
            DateTime utcDateTime = (localDateTime.Kind == DateTimeKind.Unspecified)
                ? DateTime.SpecifyKind(localDateTime, DateTimeKind.Utc) // UTC'ye dönüştür
                : localDateTime.ToUniversalTime(); // Zaten UTC ise olduğu gibi bırak

            // Randevuyu oluştur
            var appointment = new Appointment
            {
                ServiceId = serviceId,
                EmployeeId = employeeId,
                Price = service.Price,
                AppointmentDate = utcDateTime,  // UTC olarak kaydediyoruz
                DateTime = utcDateTime.TimeOfDay, // Sadece saati kaydediyoruz
                Status = AppointmentStatus.Pending,
                UserId = userId
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }






        public async Task<IActionResult> UserAppointments()
        {
            var userId = _userManager.GetUserId(User);

           
            var appointments = await _context.Appointments
               .Include(a => a.Service)
               .Where(a => a.UserId== userId)  //// Kullanıcının randevularını çekilir
               .Include(a => a.Employee)
               
               .ToListAsync();

            return View(appointments);
        }



        // Kullanıcı randevusunu iptal ettiğinde, randevu veritabanından silinecek
        [HttpPost]
        [Authorize]
        public IActionResult DeleteConfirmed(int id)
        {
            var appointment = _context.Appointments.Find(id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Kullanıcının sadece kendi randevusunu silebilmesi için kontrol
            if (appointment.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Unauthorized();
            }

            // Randevuyu iptal etme
            appointment.Status = AppointmentStatus.Cancelled; // Veya silme yerine iptal etme
            _context.Appointments.Remove(appointment);
            _context.SaveChanges();

            return RedirectToAction(nameof(UserAppointments));
        }
    }




}





