using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;

namespace Randevu_Sistemi_Kuafor.Controllers
{
    public class ImageController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private const string ApiUrl = "https://hairstyle-changer.p.rapidapi.com/huoshan/facebody/hairstyle";
        private const string ApiKey = "84d93e1cebmshf6cbb20faa8db76p1eb7a2jsnd2ea9a4a95f9";

        public ImageController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile image, int hairType = 101)
        {
            // Resim dosyası kontrolü
            if (image == null || image.Length == 0)
            {
                ModelState.AddModelError("", "Lütfen bir resim dosyası yükleyin.");
                return View("Index");
            }

            // Dosya boyutu kontrolü (5 MB sınırı)
            if (image.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("", "Yüklenen resim 5 MB'den büyük olamaz.");
                return View("Index");
            }

            // Dosya formatı kontrolü
            var allowedFormats = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
            var fileExtension = Path.GetExtension(image.FileName).ToLower();
            if (!allowedFormats.Contains(fileExtension))
            {
                ModelState.AddModelError("", "Yalnızca JPEG, JPG, PNG ve BMP formatları destekleniyor.");
                return View("Index");
            }

            // Resim çözünürlüğünü kontrol etme
            using (var imageStream = image.OpenReadStream())
            {
                var img = System.Drawing.Image.FromStream(imageStream);
                if (img.Width > 4096 || img.Height > 4096)
                {
                    ModelState.AddModelError("", "Resim çözünürlüğü 4096x4096 pikselden büyük olamaz.");
                    return View("Index");
                }
            }

            // API'ye istek gönderme
            var client = _httpClientFactory.CreateClient();
            var requestContent = new MultipartFormDataContent();

            using var stream = image.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
            requestContent.Add(streamContent, "image_target", image.FileName);
            requestContent.Add(new StringContent(hairType.ToString()), "hair_type");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://hairstyle-changer.p.rapidapi.com/huoshan/facebody/hairstyle"),
                Headers =
        {
            { "x-rapidapi-key", "84d93e1cebmshf6cbb20faa8db76p1eb7a2jsnd2ea9a4a95f9" },
            { "x-rapidapi-host", "hairstyle-changer.p.rapidapi.com" },
        },
                Content = requestContent
            };

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);

                ViewBag.ResultImage = result.data?.image;
                return View("Index");
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"API Hatası: {errorBody}");
                return View("Index");
            }
        }
    }

 }

