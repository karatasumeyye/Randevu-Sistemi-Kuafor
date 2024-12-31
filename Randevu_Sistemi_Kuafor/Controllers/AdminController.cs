using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Randevu_Sistemi_Kuafor.Models;

namespace Randevu_Sistemi_Kuafor.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {

        private readonly SalonDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(SalonDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        //SErvis EKleme
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


        //Servis Güncelleme, ajax
        [HttpGet]
        public IActionResult ServiceUpdate()
        {
            var services= _context.Services.ToList();
            return View(services);
        }



        //Servis Editleme,Ajax 
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

        //Servis Silme

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

        //Servis Silme
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


        // Employee Eklemek için user listeleme
        [HttpGet]
        public IActionResult AddEmployee()
        {
            var users = _context.Users.ToList();
            return View(users);
        }


        //Epmloyee Ekleme
        [HttpPost]
        // UserManager işlemleriasenkron olduğu için async ve await anahtar kelimeleri kullanılır
        public async Task<IActionResult> AddEmployee(string userId, string specialty, decimal salary, DateTime startDate)
        {
            // UserManager üzerinden kullanıcıyı al
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("AddEmployee");
            }

            // Kullanıcının mevcut rollerini sil
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
            {
               var removeResult = await _userManager.RemoveFromRolesAsync(user, roles);

                if (!removeResult.Succeeded)
                {
                    TempData["Error"] = "Kullanıcının mevcut rolleri kaldırılırken bir hata oluştu.";
                    return RedirectToAction("AddEmployee");
                }
            }

            // Kullanıcıya "Employee" rolü ekle
            var addToRoleResult = await _userManager.AddToRoleAsync(user, "Employee");
            if (!addToRoleResult.Succeeded)
            {
                var errorMessage = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                TempData["Error"] = $"User's role couldn't be updated: {errorMessage}";
                return RedirectToAction("AddEmployee");
            }

            // Employee kaydı oluştur
            var employee = new Employee
            {
                UserId = user.Id,
                Specialty = specialty,
                Salary = salary,
                StartDate = startDate.ToUniversalTime()
            };

            try
            {
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Employee added successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error while saving employee: {ex.Message}";
            }

            return RedirectToAction("AddEmployee", "Admin");
        }

        //Employee Tablosunndakileri listeleme
        [HttpGet]
        public IActionResult AddEmployeeService()
        {
            var employeeWithUserInfo = from e in _context.Employees
                                       join u in _context.Users on e.UserId equals u.Id
                                       select new
                                       {
                                           e.EmployeeId,
                                           UserName = u.Name,
                                           UserPhone = u.PhoneNumber,  // PhoneNumber kullanımı
                                           UserEmail = u.Email,
                                           e.Specialty,
                                           e.Salary,
                                           e.StartDate

                                       };

            return View(employeeWithUserInfo.ToList());

        }



        // Servileri Listeleme, Employee ile ilişkilendirme için 
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

        //Randevuları Listeleme
        public async Task<IActionResult> ListAppointment()
        {

            var appointments = await _context.Appointments
           .Include(a => a.Service)
           .Include(a => a.User)
           .Include(a => a.Employee)
           .ToListAsync();

            return View(appointments);
        }



    }
}
