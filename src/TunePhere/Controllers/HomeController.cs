using Microsoft.AspNetCore.Mvc;
using TunePhere.Repository;
using System.Threading.Tasks;
using TunePhere.Repository.IMPRepository;
using TunePhere.Models;
using Microsoft.EntityFrameworkCore;

namespace TunePhere.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISongRepository _songRepo;
        private readonly IPlaylistRepository _playlistRepo;
        private readonly IRemixRepository _remixRepo;
        private readonly AppDbContext _context;

        public HomeController(
            ISongRepository songRepo, 
            IPlaylistRepository playlistRepo, 
            IRemixRepository remixRepo,
            AppDbContext context)
        {
            _songRepo = songRepo;
            _playlistRepo = playlistRepo;
            _remixRepo = remixRepo;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var topSongs = await _songRepo.GetActiveSongsAsync();
            var suggestedPlaylists = await _playlistRepo.GetAllAsync(); // Changed to GetAllAsync
            var trendingRemixes = await _remixRepo.GetTopRemixesAsync();

            // Lấy danh sách nghệ sĩ phổ biến dựa trên số lượng bài hát và lượt nghe
            var popularArtists = await _context.Artists
                .Include(a => a.Songs)
                .OrderByDescending(a => a.Songs.Sum(s => s.PlayCount))
                .Take(8)
                .ToListAsync();

            // Lấy danh sách album nổi tiếng
            var popularAlbums = await _context.Albums
                .Include(a => a.Songs)
                .OrderByDescending(a => a.Songs.Sum(s => s.PlayCount))
                .Take(8)
                .ToListAsync();
            ViewBag.SuggestedPlaylists = suggestedPlaylists;
            ViewBag.TrendingRemixes = trendingRemixes;
            ViewBag.PopularArtists = popularArtists;
            ViewBag.PopularAlbums = popularAlbums;
            return View(topSongs);
        }
    }
}
