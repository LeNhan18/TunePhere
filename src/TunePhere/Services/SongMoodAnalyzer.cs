using TunePhere.Models;

namespace TunePhere.Services
{
    public class SongMoodAnalyzer
    {
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