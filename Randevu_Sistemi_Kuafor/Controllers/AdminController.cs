using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Randevu_Sistemi_Kuafor.Models;

namespace Randevu_Sistemi_Kuafor.Controllers
{
    public class AdminController : Controller
    {

        private readonly SalonDbContext _context;

        public AdminController(SalonDbContext context)
        {
            _context = context;
        }


        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ServiceAdd()
        {
            return View();
        }


        [HttpPost]
        public IActionResult ServiceAdd(Service service)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Services.Add(service);
                    int changes = _context.SaveChanges(); // Kaç satır eklendiğini kontrol et
                    TempData["msj"] = service.ServiceName + " servis başarıyla eklendi";
                    return RedirectToAction("ServiceUpdate", "Admin");
                }
                catch (Exception ex)
                {
                    TempData["msj"] = "Bir hata oluştu: " + ex.Message;
                    Console.WriteLine($"Hata: {ex.Message}");
                }
            }
            else
            {
                // ModelState hatalarını kontrol et
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["msj"] = "ModelState hatası: " + errors;
                Console.WriteLine($"ModelState Hata: {errors}");
            }

            return View(service);
          
        }

        [HttpGet]
        public IActionResult ServiceUpdate()
        {
            var services= _context.Services.ToList();
            return View(services);
        }



        //Edit Action 
        [HttpPost]
        public IActionResult Edit(Service updatedService) {
            if (ModelState.IsValid)
            {
                var service = _context.Services.FirstOrDefault(s=>s.ServiceId == updatedService.ServiceId);
                if(service == null)
                {
                    return NotFound();
                }

                // Güncellenen verileri al ve güncelle
                service.ServiceName= updatedService.ServiceName;
                service.Duration = updatedService.Duration;
                service.Price = updatedService.Price;

                _context.SaveChanges();
                return Ok();

            }

            return BadRequest();
        
        
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.Services == null)
            {
                return Problem("Entity set 'Services' is null");
            }
            var service = await _context.Services.FindAsync(id);
            if(service != null)
            {
                _context.Services.Remove(service);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ServiceUpdate", "Admin");
        }


        public async Task<IActionResult> Detail(int id)
        {
            if(id == null || _context.Services == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == id);
            if(service == null)
            {
                return NotFound();
            }

            return View(service);
        }


        [HttpGet]

        public IActionResult AddEmployee()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        [HttpPost]
        public IActionResult AddEmployee(int userId, string specialty,decimal salary, DateTime startDate)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return NotFound();
            }

            var employee = new Employee
            {
                UserId = user.UserId,
                Specialty = specialty,
                Salary=salary,
                StartDate = startDate.ToUniversalTime()
            };

            _context.Employees.Add(employee);
            _context.SaveChanges();

            TempData["Message"] = "Employee added successfully!";
            return RedirectToAction("AddEmployee", "Admin");
        }


        //Employee Tablosunndakileri listeleme
        [HttpGet]
        public IActionResult AddEmployeeService()
        {
            var employeeWithUserInfo = from e in _context.Employees
                                       join u in _context.Users on e.UserId equals u.UserId
                                       select new
                                       {
                                           e.EmployeeId,
                                           UserName = u.Name,
                                           UserPhone = u.Phone,
                                           UserEmail = u.Email,
                                           e.Specialty,
                                           e.Salary,
                                           e.StartDate

                                       };

            return View(employeeWithUserInfo.ToList());

        }



        // Servileri Listeleme
        [HttpGet]
        public IActionResult GetServices(int employeeId)
        {
            // Servis listesini getir

            var allServices = _context.Services.ToList();
            var assignedServices = _context.EmployeeServices
                .Where(es => es.EmployeeId == employeeId)
                .Select(es => es.ServiceId)
                .ToList();

            var serviceList = allServices.Select(service => new
            {
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                IsSelected = assignedServices.Contains(service.ServiceId)  // Eğer servis atanmışsa , IsSelected true olacak
            }).ToList();

            return Json(serviceList);
        }

        //Seçili Servisleri Kaydet

        [HttpPost]
        public IActionResult SaveServices(int  employeeId, List<int> serviceIds)
        {
            // Mevcut kayıtları sil
            var existingServices = _context.EmployeeServices
                                           .Where(es => es.EmployeeId == employeeId);
            _context.EmployeeServices.RemoveRange(existingServices);

            // Yeni kayıtları ekle
            var newServices = serviceIds.Select(serviceId => new EmployeeService
            {
                EmployeeId = employeeId,
                ServiceId = serviceId
            });

            _context.EmployeeServices.AddRange(newServices);
            _context.SaveChanges();

            return Json(new { success = true });
        }


        //Employee Detayları
        
        public async Task<IActionResult> EmployeeDetail(int employeeId)
        {
            var employee = _context.Employees
                .Include(e=>e.User)   //Employee ile ilişkili User dahil edilir
                .Include(e=>e.EmployeeServices)   //Employee ile ilişkili EmployeeService'leri dahil et
                .ThenInclude(es=>es.Service)
                .FirstOrDefault(e=>e.EmployeeId==employeeId);

            if(employee == null)
            {
                return NotFound();
            }

            var model = new EmployeeDetailsViewModel
            {
                Employee = employee,
                Services = employee.EmployeeServices.Select(es => es.Service).ToList()
            };


            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeDelete(int employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.User)   //User tablosu ile olan ilişkiden dolayı
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);



            if (employee == null) { 
                return NotFound();
            }

           
            // Employee kaydını silme işlemi , ilişkili olan tablolardan siliniyor
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

           
            return RedirectToAction("AddEmployeeService", "Admin");
        }




    }
}
