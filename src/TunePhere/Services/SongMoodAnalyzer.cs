using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TunePhere.Models;

namespace TunePhere.Services
{
    public class SongMoodAnalyzer
    {
        private readonly string _apiToken = "a88cc52df3a8d82cbfdfa0ead0f77a34";

        public async Task<string?> AnalyzeSongMoodAsync(string filePath)
        {
            using (var client = new HttpClient())
            using (var form = new MultipartFormDataContent())
            {
                form.Add(new StringContent(_apiToken), "api_token");
                form.Add(new StringContent("recognize"), "return");
                var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(filePath));
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/mpeg");
                form.Add(fileContent, "file", "song.mp3");

                var response = await client.PostAsync("https://api.audd.io/", form);
                var result = await response.Content.ReadAsStringAsync();

                // Parse kết quả JSON để lấy thông tin mood (nếu có)
                var json = JObject.Parse(result);
                var mood = json["result"]?["mood"]?.ToString();

                return mood; // Có thể là null nếu API không trả về mood
            }
        }

        public SongMood AnalyzeMood(float valence, float energy, float danceability, float tempo, string mode)
        {
            var mood = new SongMood
            {
                Valence = valence,
                Energy = energy,
                Danceability = danceability,
                Tempo = tempo,
                Mode = mode
            };

            // Phân tích cảm xúc dựa trên Valence và Energy
            if (valence > 0.6f && energy > 0.6f)
            {
                mood.Mood = "Vui vẻ, Sôi động";
                mood.Description = "Bài hát này có giai điệu vui vẻ, sôi động, phù hợp cho các hoạt động vui chơi và tiệc tùng.";
            }
            else if (valence > 0.6f && energy < 0.4f)
            {
                mood.Mood = "Nhẹ nhàng, Lạc quan";
                mood.Description = "Bài hát này có giai điệu nhẹ nhàng, lạc quan, phù hợp cho những khoảnh khắc thư giãn.";
            }
            else if (valence < 0.4f && energy > 0.6f)
            {
                mood.Mood = "Mạnh mẽ, Căng thẳng";
                mood.Description = "Bài hát này có giai điệu mạnh mẽ, căng thẳng, phù hợp cho các hoạt động thể thao hoặc khi cần động lực.";
            }
            else if (valence < 0.4f && energy < 0.4f)
            {
                mood.Mood = "Buồn bã, Trầm lắng";
                mood.Description = "Bài hát này có giai điệu buồn bã, trầm lắng, phù hợp cho những khoảnh khắc suy tư.";
            }
            else
            {
                mood.Mood = "Trung tính";
                mood.Description = "Bài hát này có giai điệu cân bằng, không quá vui cũng không quá buồn.";
            }

            // Bổ sung thông tin về mode
            if (mode.ToLower() == "major")
            {
                mood.Description += " Sử dụng thang âm trưởng tạo cảm giác tươi sáng hơn.";
            }
            else if (mode.ToLower() == "minor")
            {
                mood.Description += " Sử dụng thang âm thứ tạo cảm giác sâu lắng hơn.";
            }

            // Bổ sung thông tin về danceability
            if (danceability > 0.7f)
            {
                mood.Description += " Rất phù hợp để nhảy múa.";
            }
            else if (danceability < 0.3f)
            {
                mood.Description += " Ít phù hợp để nhảy múa.";
            }

            return mood;
        }
    }
} 