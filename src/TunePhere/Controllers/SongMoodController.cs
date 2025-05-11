using Microsoft.AspNetCore.Mvc;
using TunePhere.Models;
using TunePhere.Services;
using TunePhere.Repository;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Controllers
{
    public class SongMoodController : Controller
    {
        private readonly ISongRepository _songRepository;
        private readonly SongMoodAnalyzer _moodAnalyzer;

        public SongMoodController(ISongRepository songRepository, SongMoodAnalyzer moodAnalyzer)
        {
            _songRepository = songRepository;
            _moodAnalyzer = moodAnalyzer;
        }

        public IActionResult Analyze(int songId)
        {
            var song = _songRepository.GetByIdAsync(songId);
            if (song == null)
            {
                return NotFound();
            }

            // Giả lập dữ liệu từ Spotify API (trong thực tế, bạn sẽ gọi Spotify API)
            var mood = _moodAnalyzer.AnalyzeMood(
                valence: 0.7f,      // Ví dụ: bài hát khá vui vẻ
                energy: 0.8f,       // Ví dụ: bài hát khá sôi động
                danceability: 0.75f, // Ví dụ: bài hát dễ nhảy múa
                tempo: 120f,        // Ví dụ: tempo trung bình
                mode: "major"       // Ví dụ: thang âm trưởng
            );

            return View(mood);
        }
    }
} 