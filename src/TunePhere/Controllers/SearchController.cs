using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using System.Diagnostics;

namespace TunePhere.Controllers
{
    public class SearchController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SearchController> _logger;

        public SearchController(AppDbContext context, ILogger<SearchController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return View(new SearchResults());
            }

            var userId = User.Identity.IsAuthenticated ? User.Identity.Name : null;

            // Tìm kiếm nghệ sĩ
            var artist = await _context.Artists
                .Include(a => a.Songs)
                .Include(a => a.Albums)
                .Include(a => a.Followers)
                .FirstOrDefaultAsync(a => EF.Functions.Like(a.ArtistName, $"%{searchTerm}%"));

            if (artist != null)
            {
                // Kiểm tra trạng thái like của các bài hát
                if (userId != null)
                {
                    foreach (var song in artist.Songs)
                    {   
                        song.IsLiked = await _context.UserFavoriteSongs
                            .AnyAsync(sl => sl.SongId == song.SongId && sl.UserId == userId);
                    }
                }

                return View("ArtistResult", artist);
            }

            // Tìm kiếm bài hát
            var songs = await _context.Songs
                .Include(s => s.Artists)
                .Include(s => s.Albums)
                .Where(s => EF.Functions.Like(s.Title, $"%{searchTerm}%"))
                .ToListAsync();

            // Kiểm tra trạng thái like của các bài hát
            if (userId != null)
            {
                foreach (var song in songs)
                {
                    song.IsLiked = await _context.UserFavoriteSongs
                        .AnyAsync(sl => sl.SongId == song.SongId && sl.UserId == userId);
                }
            }

            var results = new SearchResults
            {
                Songs = songs,
                SearchTerm = searchTerm
            };

            _logger.LogInformation($"Found {songs.Count} songs for search term: {searchTerm}");

            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> SearchSongs(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return Json(new { success = false, message = "Vui lòng nhập từ khóa tìm kiếm" });
                }

                searchTerm = searchTerm.ToLower().Trim();
                _logger.LogInformation($"Searching songs for: {searchTerm}");

                var songs = await _context.Songs
                    .Include(s => s.Artists)
                    .AsNoTracking()
                    .Where(s => s.Title.ToLower().Contains(searchTerm) ||
                               s.Genre.ToLower().Contains(searchTerm) ||
                               (s.Artists != null && s.Artists.ArtistName.ToLower().Contains(searchTerm)))
                    .Select(s => new {
                        s.SongId,
                        s.Title,
                        ArtistName = s.Artists != null ? s.Artists.ArtistName : "Unknown",
                        s.ImageUrl,
                        s.Genre,
                        s.Duration
                    })
                    .ToListAsync();

                return Json(new { success = true, data = songs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching songs");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tìm kiếm bài hát" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchArtists(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return Json(new { success = false, message = "Vui lòng nhập từ khóa tìm kiếm" });
                }

                searchTerm = searchTerm.ToLower().Trim();
                _logger.LogInformation($"Searching artists for: {searchTerm}");

                var artists = await _context.Artists
                    .Include(a => a.Songs)
                    .Include(a => a.Albums)
                    .AsNoTracking()
                    .Where(a => a.ArtistName.ToLower().Contains(searchTerm) ||
                               (a.Bio != null && a.Bio.ToLower().Contains(searchTerm)))
                    .Select(a => new {
                        a.ArtistId,
                        a.ArtistName,
                        a.ImageUrl,
                        a.Bio,
                        SongCount = a.Songs.Count,
                        AlbumCount = a.Albums.Count,
                        Songs = a.Songs.Select(s => new {
                            s.SongId,
                            s.Title,
                            s.ImageUrl,
                            s.Genre,
                            s.Duration
                        }),
                        Albums = a.Albums.Select(alb => new {
                            alb.AlbumId,
                            alb.AlbumName,
                            alb.ImageUrl,
                            alb.ReleaseDate
                        })
                    })
                    .ToListAsync();

                return Json(new { success = true, data = artists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching artists");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tìm kiếm nghệ sĩ" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Like(int songId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này" });
            }

            var userId = User.Identity.Name;
            var existingLike = await _context.UserFavoriteSongs
                .FirstOrDefaultAsync(sl => sl.SongId == songId && sl.UserId == userId);

            var song = await _context.Songs.FindAsync(songId);
            if (song == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài hát" });
            }

            if (existingLike != null)
            {
                _context.UserFavoriteSongs.Remove(existingLike);
                song.LikeCount--;
                await _context.SaveChangesAsync();
                return Json(new { success = true, isLiked = false });
            }
            else
            {
                var songLike = new UserFavoriteSong
                {
                    SongId = songId,
                    UserId = userId
                };
                _context.UserFavoriteSongs.Add(songLike);
                song.LikeCount++;
                await _context.SaveChangesAsync();
                return Json(new { success = true, isLiked = true });
            }
        }

        [Route("api/search/suggestions")]
        public async Task<IActionResult> GetSearchSuggestions([FromQuery] string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
            {
                return Json(new
                {
                    artists = new List<object>(),
                    songs = new List<object>(),
                    albums = new List<object>()
                });
            }

            var artists = await _context.Artists
                .Where(a => a.ArtistName.Contains(term))
                .Take(3)
                .Select(a => new {
                    id = a.ArtistId,
                    name = a.ArtistName,
                    imageUrl = a.ImageUrl
                })
                .ToListAsync();

            var songs = await _context.Songs
                .Include(s => s.Artists)
                .Where(s => s.Title.Contains(term))
                .Take(3)
                .Select(s => new {
                    id = s.SongId,
                    title = s.Title,
                    artistName = s.Artists.ArtistName,
                    imageUrl = s.ImageUrl
                })
                .ToListAsync();

            var albums = await _context.Albums
                .Include(a => a.Artists)
                .Where(a => a.AlbumName.Contains(term))
                .Take(3)
                .Select(a => new {
                    id = a.AlbumId,
                    name = a.AlbumName,
                    artistName = a.Artists.ArtistName,
                    imageUrl = a.ImageUrl
                })
                .ToListAsync();

            return Json(new { artists, songs, albums });
        }

        [HttpGet("Search/Artist/{id}")]
        public async Task<IActionResult> ArtistDetail(int id)
        {
            var artist = await _context.Artists
                .Include(a => a.Songs)
                .Include(a => a.Albums)
                .Include(a => a.Followers)
                .FirstOrDefaultAsync(a => a.ArtistId == id);

            if (artist == null)
            {
                return NotFound();
            }

            return View("ArtistResult", artist);
        }

        [HttpGet("Search/Song/{id}")]
        public async Task<IActionResult> SongDetail(int id)
        {
            var song = await _context.Songs
                .Include(s => s.Artists)
                .Include(s => s.Albums)
                .FirstOrDefaultAsync(s => s.SongId == id);

            if (song == null)
            {
                return NotFound();
            }

            var searchResults = new SearchResults
            {
                Songs = new List<Song> { song },
                SearchTerm = song.Title
            };

            return View("Index", searchResults);
        }

        [HttpGet("Search/Album/{id}")]
        public async Task<IActionResult> AlbumDetail(int id)
        {
            var album = await _context.Albums
                .Include(a => a.Artists)
                .Include(a => a.Songs)
                .FirstOrDefaultAsync(a => a.AlbumId == id);

            if (album == null)
            {
                return NotFound();
            }

            return View("AlbumResult", album);
        }
    }
}