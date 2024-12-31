using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Randevu_Sistemi_Kuafor.Models;
using System.Net.Http.Json;

namespace Randevu_Sistemi_Kuafor.Controllers
{
    public class EmployeeLeaveConsumeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SalonDbContext _context;
        public EmployeeLeaveConsumeController(UserManager<ApplicationUser> userManager, SalonDbContext context)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5123/api/");
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Kullanıcıyı al
            var user = await _userManager.GetUserAsync(User);

            // Eğer kullanıcı varsa
            if (user != null)
            {
                // UserId ile ilişkili Employee kaydını almak için EmployeeId'yi alıyoruz
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.UserId == user.Id);

                if (employee != null)
                {
                    // EmployeeId'yi kullanarak sadece o Employee'ye ait izinleri alıyoruz
                    List<EmployeeLeave> leaves = new List<EmployeeLeave>();

                    // EmployeeLeave API'den sadece giriş yapan kullanıcıya ait izinleri almak için
                    var response = await _httpClient.GetAsync($"EmployeeLeaveApi?employeeId={employee.EmployeeId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonData = await response.Content.ReadAsStringAsync();
                        leaves = JsonConvert.DeserializeObject<List<EmployeeLeave>>(jsonData);
                    }

                    return View(leaves);
                }
            }

            // Kullanıcı bulunmazsa, model hatası ekleyebiliriz veya başka bir işlem yapılabilir.
            ModelState.AddModelError(string.Empty, "User not found.");
            return View();
        }


        public IActionResult Create()
        {
            return View();
        }




        [HttpPost]
        public async Task<IActionResult> Create(EmployeeLeave leave)
        {

            // Kullanıcıyı al
            var user = await _userManager.GetUserAsync(User);

            // Eğer kullanıcı varsa
            if (user != null)
            {
                // UserId ile ilişkili Employee kaydını almak için JOIN sorgusu
                var employeeWithUserInfo = await (from e in _context.Employees
                                                  join u in _context.Users on e.UserId equals u.Id
                                                  where u.Id == user.Id  // Kullanıcının ID'si ile eşleşen Employee kaydını al
                                                  select new
                                                  {
                                                      e.EmployeeId,
                                                      e.UserId // Diğer Employee bilgilerini de buradan alabilirsiniz
                                                  }).FirstOrDefaultAsync();

                // Eğer employee bulunursa
                if (employeeWithUserInfo != null)
                {
                    // EmployeeId'yi leave kaydına ekle
                    leave.EmployeeId = employeeWithUserInfo.EmployeeId;
                }
                else
                {
                    // Eğer Employee bulunamazsa, model hatası ekle
                    ModelState.AddModelError(string.Empty, "Employee not found for the current user.");
                    return View(leave);
                }
            }
            else
            {
                // Kullanıcı bulunamazsa, model hatası ekle
                ModelState.AddModelError(string.Empty, "User not found.");
                return View(leave);
            }

            // Tarihleri UTC'ye dönüştür
            leave.LeaveStartDate = leave.LeaveStartDate.ToUniversalTime();
            leave.LeaveEndDate = leave.LeaveEndDate.ToUniversalTime();

            // API'ye veri gönder
            var response = await _httpClient.PostAsJsonAsync("EmployeeLeaveApi", leave);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"An error occurred while creating the leave: {errorMessage}");
                return View(leave);
            }
        }





        // GET: EmployeeLeave/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"EmployeeLeaveApi/{id}");

            if (response.IsSuccessStatusCode)
            {
                // Gelen JSON verisini string olarak al
                var jsonData = await response.Content.ReadAsStringAsync();

                // Json verisini EmployeeLeave modeline dönüştür
                var leave = JsonConvert.DeserializeObject<EmployeeLeave>(jsonData); // JsonConvert ile deserialize ediliyor

                // EmployeeId'yi doğrudan modelden alabiliriz
                var employeeId = leave.EmployeeId;

                return View(leave); // Bu modelin içinde EmployeeId zaten var
            }

            return NotFound();
        }



        // POST: EmployeeLeave/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeLeave leave)
        {
            // Eğer gelen leave modelinde EmployeeId varsa, bu değeri kullanacağız
            if (id != leave.LeaveId)
            {
                return NotFound();
            }

            // Model geçerli ise işlemi yap
            if (ModelState.IsValid)
            {
                // Burada EmployeeId zaten mevcut ve değişmeyecek
                // PUT isteği ile bu modeli API'ye göndereceğiz
                var response = await _httpClient.PutAsJsonAsync($"EmployeeLeaveApi/{id}", leave);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the leave.");
                }
            }

            // Model hatası varsa tekrar view döndür
            return View(leave);
        }





        // GET: EmployeeLeave/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"EmployeeLeaveApi/{id}");

            if (response.IsSuccessStatusCode)
            {
                // Gelen JSON verisini string olarak al
                var jsonData = await response.Content.ReadAsStringAsync();

                // Json verisini EmployeeLeave modeline dönüştür
                var leave = JsonConvert.DeserializeObject<EmployeeLeave>(jsonData); // JsonConvert ile deserialize ediliyor

                return View(leave);
            }

            return NotFound();
        }

        // GET: EmployeeLeave/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"EmployeeLeaveApi/{id}");

            if (response.IsSuccessStatusCode)
            {
                // Gelen JSON verisini string olarak al
                var jsonData = await response.Content.ReadAsStringAsync();

                // Json verisini EmployeeLeave modeline dönüştür
                var leave = JsonConvert.DeserializeObject<EmployeeLeave>(jsonData); // JsonConvert ile deserialize ediliyor

                return View(leave);
            }

            return NotFound();
        }


        // POST: EmployeeLeave/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"EmployeeLeaveApi/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "An error occurred while deleting the leave.");
            return RedirectToAction(nameof(Delete), new { id });
        }



    }

}

