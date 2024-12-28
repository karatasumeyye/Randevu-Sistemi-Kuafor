using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Randevu_Sistemi_Kuafor.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Randevu_Sistemi_Kuafor.Controllers
{
    public class HomeController : Controller
    {

        private readonly SalonDbContext _context;
    
      

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, SalonDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult ListServices()
        {
            var services = _context.Services.ToList();
            return View(services);
        }



    }
}
