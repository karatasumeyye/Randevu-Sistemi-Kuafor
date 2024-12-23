using Microsoft.AspNetCore.Mvc;
using Randevu_Sistemi_Kuafor.Models;
using System.Linq;

namespace Randevu_Sistemi_Kuafor.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly SalonDbContext _context;

        public AppointmentController(SalonDbContext context)
        {
            _context = context;
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
        

    }
}
