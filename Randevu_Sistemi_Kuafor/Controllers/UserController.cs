using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Randevu_Sistemi_Kuafor.Models;

namespace Randevu_Sistemi_Kuafor.Controllers
{
    public class UserController : Controller
    {
      
        private readonly SalonDbContext _context;

        public UserController(SalonDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Users.Add(user);
                    int changes = _context.SaveChanges(); // Kaç satır eklendiğini kontrol et
                    TempData["msj"] = user.Name + " başarıyla kaydoldunuz. Eklendi: " + changes;
                    return RedirectToAction("Index","Home");
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

            return View(user);
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user= _context.Users.FirstOrDefault(u=>u.Email== email && u.Password== password);

            if(user !=null)
            {
                //Login başarılı
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserName", user.Name);
                TempData["msj"] = user.Name + " kullanıcısı login oldu";

                return RedirectToAction("Index", "Home");
            }

            TempData["msj"] = "Kullanıcı Adı/Şifre Hatalı";
            return RedirectToAction("Login");


        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
