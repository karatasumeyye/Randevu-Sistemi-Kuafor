using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Randevu_Sistemi_Kuafor.Models;

namespace Randevu_Sistemi_Kuafor.Controllers
{
    public class UserController : Controller
    {
      
        private readonly SalonDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;  // UserManager tanımlanıyor
        private readonly RoleManager<ApplicationRole> _roleManager;  // RoleManager ekleniyor

        public UserController(SalonDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;  // Constructor üzerinden UserManager alınır
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Users.Add(user);
                    int changes = _context.SaveChanges(); // Kaç satır eklendiğini kontrol et

                    if (changes > 0)
                    {
                        var result = await _userManager
                            .CreateAsync(user, user.Email);

                        if (result.Succeeded)
                        {
                            var roleResult = await _userManager
                                .AddToRoleAsync(user, "User");
                            if(roleResult.Succeeded)
                                return RedirectToAction("Index", "Home");
                        }



                        var appUser = await _userManager.FindByEmailAsync(user.Email);

                        if (appUser != null)
                        {
                            // 'User' rolü veritabanında var mı kontrol et
                            var roleExists = await _roleManager.RoleExistsAsync("User");

                            if (!roleExists)
                            {
                                // Eğer 'User' rolü yoksa, onu oluştur
                                var role = new ApplicationRole { Name = "User" };
                                await _roleManager.CreateAsync(role);  // Rolü veritabanına ekle
                            }

                            // Kullanıcıya 'User' rolünü ata
                            await _userManager.AddToRoleAsync(appUser, "User");
                        }

                        TempData["msj"] = user.UserName + " başarıyla kaydoldunuz. Eklendi: " + changes;
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["msj"] = "Kullanıcı kaydı başarısız.";
                    }
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
            var user= _context.Users.FirstOrDefault(u=>u.Email== email && u.PasswordHash== password);

            if(user !=null)
            {
                //Login başarılı
                HttpContext.Session.SetString("UserId", user.Id.ToString());
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
