using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Randevu_Sistemi_Kuafor.Models;

namespace Randevu_Sistemi_Kuafor.Controllers
{
    public class EmployeeLeaveConsumeController : Controller
    {
        private readonly HttpClient _httpClient;

        public EmployeeLeaveConsumeController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5123/api/");
        }

        public async Task<IActionResult> Index()
        {
            List<EmployeeLeave> leaves = new List<EmployeeLeave>();
            var response = await _httpClient.GetAsync("EmployeeLeaveApi");

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                leaves = JsonConvert.DeserializeObject<List<EmployeeLeave>>(jsonData);
            }

            return View(leaves);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeLeave leave)
                {
            // Tarihleri UTC'ye dönüştür
            leave.LeaveStartDate = leave.LeaveStartDate.ToUniversalTime();
            leave.LeaveEndDate = leave.LeaveEndDate.ToUniversalTime();

            var response = await _httpClient.PostAsJsonAsync("EmployeeLeaveApi", leave);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(leave);
        }



    }
}