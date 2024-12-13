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


    }
}
