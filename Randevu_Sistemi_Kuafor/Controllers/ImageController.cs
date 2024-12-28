using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Randevu_Sistemi_Kuafor.Controllers
{
    public class ImageController : Controller
    {
        private const string ApiKey = "r8_YR4XnzVFn1dIzoPmi7GB7m27vmpK5920DmtV6";
        private const string ReplicateUrl = "https://api.replicate.com/v1/predictions";

        public IActionResult Index()
        {
            return View();
        }

        private async Task<string> SendImageToReplicate(string imageUrl, string colorDescription, string hairstyleDescription)
        {
            using (var client = new HttpClient())
            {
                // Authorization başlığını ekle
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

                var requestData = new
                {
                    version = "b95cb2a16763bea87ed7ed851d5a3ab2f4655e94bcfb871edba029d4814fa587",
                    input = new
                    {
                        image = imageUrl,
                        color_description = colorDescription,
                        hairstyle_description = hairstyleDescription
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // POST isteği gönder
                var response = await client.PostAsync(ReplicateUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    return $"Error: {response.ReasonPhrase}";
                }

                // POST yanıtını oku
                var postResponse = await response.Content.ReadAsStringAsync();
                var postResult = JsonConvert.DeserializeObject<dynamic>(postResponse);
                string predictionId = postResult.id;

                // İşlemin tamamlanmasını bekle
                string getUrl = $"{ReplicateUrl}/{predictionId}";
                while (true)
                {
                    var getResponse = await client.GetAsync(getUrl);
                    var getContent = await getResponse.Content.ReadAsStringAsync();
                    var getResult = JsonConvert.DeserializeObject<dynamic>(getContent);

                    if (getResult.status == "succeeded")
                    {
                        return getResult.output.ToString();
                    }
                    else if (getResult.status == "failed")
                    {
                        return $"Error: {getResult.error}";
                    }

                    // Bekleme süresi
                    await Task.Delay(2000);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file, string colorDescription, string hairstyleDescription)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Görseli sunucuya yükle
            var uploadDirectory = Path.Combine("wwwroot", "uploads");
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            var filePath = Path.Combine(uploadDirectory, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"http://localhost:5123/uploads/{file.FileName}";
            var result = await SendImageToReplicate(imageUrl, colorDescription, hairstyleDescription);

            if (result.StartsWith("Error:"))
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
