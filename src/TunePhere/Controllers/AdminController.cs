using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TunePhere.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ISongRepository _songRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IArtistRepository _artistRepository;
        private readonly AppDbContext _context;

        public AdminController(
            UserManager<AppUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            ISongRepository songRepository,
            IPlaylistRepository playlistRepository,
            IWebHostEnvironment webHostEnvironment,
            IArtistRepository artistRepository,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _songRepository = songRepository;
            _playlistRepository = playlistRepository;
            _webHostEnvironment = webHostEnvironment;
            _artistRepository = artistRepository;
            _context = context;
        }

        // GET: /Admin
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (_songRepository == null || _playlistRepository == null)
                {
                    ViewBag.Error = "One or more repositories are null";
                    return View();
                }

                // Thống kê tổng quan
                var userCount = await _userManager.Users.CountAsync();
                ViewBag.TotalUsers = userCount;

                var songs = await _songRepository.GetAllAsync();
                var songsList = songs?.ToList() ?? new List<Song>();
                ViewBag.TotalSongs = songsList.Count;

                var playlists = await _playlistRepository.GetAllAsync();
                var playlistsList = playlists?.ToList() ?? new List<Playlist>();
                ViewBag.TotalPlaylists = playlistsList.Count;

                // Tính tổng lượt nghe từ PlayCount của tất cả bài hát
                ViewBag.TotalPlays = songsList.Sum(s => s.PlayCount);

                // Top bài hát dựa trên PlayCount
                var topSongs = songsList
                    .OrderByDescending(s => s.PlayCount)
                    .Take(5)
                    .Select(s => new { Title = s.Title, PlayCount = s.PlayCount })
                    .ToList();

                ViewBag.TopSongs = topSongs;

                // Thống kê theo tháng và ngày
                GetMonthlyStats(songsList);
                GetDailyStats(songsList);

                return View();
            }
            catch (Exception error)
            {
                ViewBag.Error = $"Có lỗi xảy ra khi tải dữ liệu dashboard: {error.Message}";
                return View();
            }
        }

        private async Task GetDailyStats(List<Song> songsList)
        {
            try
            {
                var last15Days = Enumerable.Range(0, 15)
                    .Select(i => DateTime.Now.Date.AddDays(-i))
                    .OrderBy(d => d)
                    .ToList();

                var dailyStats = new List<DailyStats>();

                foreach (var day in last15Days)
                {
                    // Lấy số lượt nghe trong ngày từ bảng PlayHistories
                    var dailyPlays = await _context.PlayHistories
                        .Where(ph => ph.PlayedAt.Date == day.Date)
                        .CountAsync();

                    // Lấy số người dùng hoạt động (distinct users có PlayHistory trong ngày)
                    var activeUsers = await _context.PlayHistories
                        .Where(ph => ph.PlayedAt.Date == day.Date)
                        .Select(ph => ph.UserId)
                        .Distinct()
                        .CountAsync();

                    // Lấy số bài hát mới upload trong ngày
                    var newSongs = await _context.Songs
                        .Where(s => s.UploadDate.Date == day.Date)
                        .CountAsync();

                    // Tính thời gian nghe trung bình (phút) từ PlayHistories
                    var avgListeningTime = await _context.PlayHistories
                        .Where(ph => ph.PlayedAt.Date == day.Date)
                        .Select(ph => ph.Song.Duration.TotalMinutes)
                        .DefaultIfEmpty(0)
                        .AverageAsync();

                    // Tính % tăng trưởng so với ngày trước
                    var previousDayPlays = await _context.PlayHistories
                        .Where(ph => ph.PlayedAt.Date == day.AddDays(-1).Date)
                        .CountAsync();

                    double growthPercent = 0;
                    if (previousDayPlays > 0)
                    {
                        growthPercent = Math.Round((dailyPlays - previousDayPlays) * 100.0 / previousDayPlays, 1);
                    }

                    dailyStats.Add(new DailyStats
                    {
                        Date = day.ToString("dd/MM/yyyy"),
                        DayOfWeek = day.ToString("dddd", new System.Globalization.CultureInfo("vi-VN")),
                        PlayCount = dailyPlays,
                        ActiveUsers = activeUsers,
                        NewSongs = newSongs,
                        AverageListeningTime = (int)avgListeningTime,
                        Growth = growthPercent
                    });
                }

                ViewBag.DailyStats = dailyStats;
                ViewBag.DailyPlayCounts = dailyStats.Select(d => d.PlayCount).ToList();
                ViewBag.Days = dailyStats.Select(d => d.Date).ToList();
            }
            catch (Exception error)
            {
                Console.WriteLine($"Lỗi khi tạo thống kê ngày: {error.Message}");
            }
        }

        private async Task GetMonthlyStats(List<Song> songsList)
        {
            try
            {
                var last6Months = Enumerable.Range(0, 6)
                    .Select(i => DateTime.Now.AddMonths(-i))
                    .OrderBy(d => d)
                    .ToList();

                var monthlyStats = new List<MonthlyStats>();

                foreach (var month in last6Months)
                {
                    var startDate = new DateTime(month.Year, month.Month, 1);
                    var endDate = startDate.AddMonths(1).AddDays(-1);

                    // Lấy số lượt nghe trong tháng
                    var monthlyPlays = await _context.PlayHistories
                        .Where(ph => ph.PlayedAt >= startDate && ph.PlayedAt <= endDate)
                        .CountAsync();

                    // Lấy số người dùng hoạt động trong tháng
                    var activeUsers = await _context.PlayHistories
                        .Where(ph => ph.PlayedAt >= startDate && ph.PlayedAt <= endDate)
                        .Select(ph => ph.UserId)
                        .Distinct()
                        .CountAsync();

                    // Lấy số bài hát mới trong tháng
                    var newSongs = await _context.Songs
                        .Where(s => s.UploadDate >= startDate && s.UploadDate <= endDate)
                        .CountAsync();

                    // Tính thời gian nghe trung bình
                    var avgListeningTime = await _context.PlayHistories
                        .Where(ph => ph.PlayedAt >= startDate && ph.PlayedAt <= endDate)
                        .Select(ph => ph.Song.Duration.TotalMinutes)
                        .DefaultIfEmpty(0)
                        .AverageAsync();

                    // Tính % tăng trưởng so với tháng trước
                    var previousMonth = month.AddMonths(-1);
                    var previousMonthStart = new DateTime(previousMonth.Year, previousMonth.Month, 1);
                    var previousMonthEnd = previousMonthStart.AddMonths(1).AddDays(-1);

                    var previousMonthPlays = await _context.PlayHistories
                        .Where(ph => ph.PlayedAt >= previousMonthStart && ph.PlayedAt <= previousMonthEnd)
                        .CountAsync();

                    double growthPercent = 0;
                    if (previousMonthPlays > 0)
                    {
                        growthPercent = Math.Round((monthlyPlays - previousMonthPlays) * 100.0 / previousMonthPlays, 1);
                    }

                    monthlyStats.Add(new MonthlyStats
                    {
                        Month = month.ToString("MM/yyyy"),
                        PlayCount = monthlyPlays,
                        ActiveUsers = activeUsers,
                        NewSongs = newSongs,
                        AverageListeningTime = (int)avgListeningTime,
                        Growth = growthPercent
                    });
                }

                ViewBag.MonthlyStats = monthlyStats;
                ViewBag.PlayCounts = monthlyStats.Select(m => m.PlayCount).ToList();
                ViewBag.Months = monthlyStats.Select(m => m.Month).ToList();
            }
            catch (Exception error)
            {
                Console.WriteLine($"Lỗi khi tạo thống kê tháng: {error.Message}");
            }
        }
        // Chỉ truy cập được endpoint này nếu chưa có tài khoản admin
        [AllowAnonymous]
        public async Task<IActionResult> CreateAdminAccount()
        {
            // Kiểm tra xem đã có Role Administrator chưa
            if (!await _roleManager.RoleExistsAsync("Administrator"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Administrator"));
            }

            // Kiểm tra xem đã có Role User chưa
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Kiểm tra xem đã có user admin chưa
            var adminEmail = "TunePhereAdmin@gmail.com";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Tạo tài khoản admin mới
                var admin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrator",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(admin, "adTunePhere@123");

                if (result.Succeeded)
                {
                    // Gán vai tròng Administrator
                    await _userManager.AddToRoleAsync(admin, "Administrator");
                    return View("AdminCreated");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            else
            {
                // Kiểm tra xem admin đã được gán role Administrator chưa
                if (!await _userManager.IsInRoleAsync(adminUser, "Administrator"))
                {
                    await _userManager.AddToRoleAsync(adminUser, "Administrator");
                }

                return View("AdminExists");
            }

            return View();
        }

        // GET: /Admin/Users
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false });
            }

            return Json(new { 
                success = true,
                id = user.Id,
                fullName = user.FullName,
                email = user.Email
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string fullName, string email, string password)
        {
            try
            {
                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Thêm người dùng thành công";
                    return RedirectToAction(nameof(Users));
                }

                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi thêm người dùng";
                return RedirectToAction(nameof(Users));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(string id, string fullName, string email)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng";
                    return RedirectToAction(nameof(Users));
                }

                user.FullName = fullName;
                user.Email = email;
                user.UserName = email;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Cập nhật thông tin thành công";
                    return RedirectToAction(nameof(Users));
                }

                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật thông tin";
                return RedirectToAction(nameof(Users));
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
                }

                // Kiểm tra xem user có phải là admin không
                if (await _userManager.IsInRoleAsync(user, "Administrator"))
                {
                    return Json(new { success = false, message = "Không thể xóa tài khoản Administrator" });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 1. Xóa PlayHistories trước
                    var playHistories = await _context.PlayHistories
                        .Where(p => p.UserId == id)
                        .ToListAsync();
                    _context.PlayHistories.RemoveRange(playHistories);
                    await _context.SaveChangesAsync();

                    // 2. Xóa các bản ghi liên quan đến Artists và Songs
                    var artists = await _context.Artists.Where(a => a.userId == id).ToListAsync();
                    foreach (var artist in artists)
                    {
                        // Lấy danh sách songs của artist
                        var songs = await _context.Songs
                            .Where(s => s.ArtistId == artist.ArtistId)
                            .ToListAsync();

                        foreach (var song in songs)
                        {
                            // 2.1 Xóa PlayHistories của các bài hát
                            var songPlayHistories = await _context.PlayHistories
                                .Where(ph => ph.SongId == song.SongId)
                                .ToListAsync();
                            _context.PlayHistories.RemoveRange(songPlayHistories);

                            // 2.2 Xóa Lyrics
                            var lyrics = await _context.Lyrics
                                .Where(l => l.SongId == song.SongId)
                                .ToListAsync();
                            _context.Lyrics.RemoveRange(lyrics);

                            // 2.3 Xóa PlaylistSongs
                            var playlistSongs = await _context.PlaylistSongs
                                .Where(ps => ps.SongId == song.SongId)
                                .ToListAsync();
                            _context.PlaylistSongs.RemoveRange(playlistSongs);

                            // 2.4 Xóa UserFavoriteSongs
                            var favoriteSongs = await _context.UserFavoriteSongs
                                .Where(fs => fs.SongId == song.SongId)
                                .ToListAsync();
                            _context.UserFavoriteSongs.RemoveRange(favoriteSongs);
                        }

                        // 2.5 Xóa Songs
                        _context.Songs.RemoveRange(songs);

                        // 2.6 Xóa Albums
                        var albums = await _context.Albums
                            .Where(a => a.ArtistId == artist.ArtistId)
                            .ToListAsync();
                        _context.Albums.RemoveRange(albums);
                    }

                    // 2.7 Xóa Artists
                    _context.Artists.RemoveRange(artists);

                    // 3. Xóa các bản ghi khác liên quan đến user
                    // 3.1 UserPreferences
                    var preferences = await _context.UserPreferences
                        .Where(p => p.UserId == id)
                        .ToListAsync();
                    _context.UserPreferences.RemoveRange(preferences);

                    // 3.2 UserFavoriteSongs
                    var userFavoriteSongs = await _context.UserFavoriteSongs
                        .Where(f => f.UserId == id)
                        .ToListAsync();
                    _context.UserFavoriteSongs.RemoveRange(userFavoriteSongs);

                    // 3.3 UserFollowers
                    var followers = await _context.UserFollowers
                        .Where(f => f.FollowerId == id || f.FollowingId == id)
                        .ToListAsync();
                    _context.UserFollowers.RemoveRange(followers);

                    // 3.4 ArtistFollowers
                    var artistFollowers = await _context.ArtistFollowers
                        .Where(f => f.UserId == id)
                        .ToListAsync();
                    _context.ArtistFollowers.RemoveRange(artistFollowers);

                    // 3.5 ListeningRoomParticipants
                    var roomParticipants = await _context.ListeningRoomParticipants
                        .Where(p => p.UserId == id)
                        .ToListAsync();
                    _context.ListeningRoomParticipants.RemoveRange(roomParticipants);

                    // 3.6 ListeningRooms
                    var rooms = await _context.ListeningRooms
                        .Where(r => r.CreatorId == id)
                        .ToListAsync();
                    _context.ListeningRooms.RemoveRange(rooms);

                    // 3.7 Playlists và PlaylistSongs
                    var playlists = await _context.Playlists
                        .Where(p => p.UserId == id)
                        .ToListAsync();
                    _context.Playlists.RemoveRange(playlists);

                    // 3.8 Remixes
                    var remixes = await _context.Remixes
                        .Where(r => r.UserId == id)
                        .ToListAsync();
                    _context.Remixes.RemoveRange(remixes);

                    // 3.9 ChatMessages
                    var chatMessages = await _context.ChatMessages
                        .Where(m => m.SenderId == id)
                        .ToListAsync();
                    _context.ChatMessages.RemoveRange(chatMessages);

                    await _context.SaveChangesAsync();

                    // 4. Cuối cùng xóa user
                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        await transaction.CommitAsync();
                        return Json(new { success = true });
                    }

                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Có lỗi xảy ra khi xóa người dùng: " + ex.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa người dùng: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Songs()
        {
            var songs = await _songRepository.GetAllAsync();
            ViewBag.Artists = await _artistRepository.GetAllAsync();
            return View(songs);
        }

        [HttpGet]
        public async Task<IActionResult> GetSong(int id)
        {
            var song = await _songRepository.GetByIdAsync(id);
            if (song == null)
            {
                return Json(new { success = false });
            }

            var artist = await _artistRepository.GetByIdAsync(song.ArtistId);

            return Json(new
            {
                success = true,
                id = song.SongId,
                title = song.Title,
                genre = song.Genre,
                duration = song.Duration,
                imageUrl = song.ImageUrl,
                fileUrl = song.FileUrl,
                artistId = song.ArtistId,
                artistName = artist?.ArtistName ?? "Không xác định",
                isActive = song.IsActive,
                videoUrl = song.VideoUrl
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetArtists()
        {
            try
            {
                var artists = await _artistRepository.GetAllAsync();
                return Json(new { success = true, data = artists.Select(a => new { 
                    id = a.ArtistId,
                    name = a.ArtistName,
                    imageUrl = a.ImageUrl,
                    coverImageUrl = a.CoverImageUrl,
                    bio = a.Bio,
                    followersCount = a.GetFollowersCount()
                })});
            }
            catch (Exception error)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách nghệ sĩ" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSong(
            string title,
            string genre,
            int artistId,
            IFormFile audioFile,
            IFormFile image,
            string? videoUrl = null)
        {
            try
            {
                // Xử lý upload ảnh
                string imageUrl = await UploadFile(image, "images/songs");
                
                // Xử lý upload audio
                string fileUrl = await UploadFile(audioFile, "audio");

                // Lấy thời lượng của file audio
                TimeSpan duration = GetAudioDuration(audioFile);

                var song = new Song
                {
                    Title = title,
                    Genre = genre,
                    Duration = duration,
                    FileUrl = fileUrl,
                    ImageUrl = imageUrl,
                    ArtistId = artistId,
                    VideoUrl = videoUrl,
                    FileType = Path.GetExtension(audioFile.FileName),
                    IsActive = true
                };

                await _songRepository.AddAsync(song);
                TempData["Success"] = "Thêm bài hát thành công";
                return RedirectToAction(nameof(Songs));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi thêm bài hát: " + ex.Message;
                return RedirectToAction(nameof(Songs));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditSong(
            int id,
            string title,
            string genre,
            int artistId,
            bool isActive,
            string? videoUrl,
            IFormFile? audioFile,
            IFormFile? image)
        {
            try
            {
                var song = await _songRepository.GetByIdAsync(id);
                if (song == null)
                {
                    TempData["Error"] = "Không tìm thấy bài hát";
                    return RedirectToAction(nameof(Songs));
                }

                // Cập nhật thông tin cơ bản
                song.Title = title;
                song.Genre = genre;
                song.ArtistId = artistId;
                song.IsActive = isActive;
                song.VideoUrl = videoUrl;
                song.UpdatedAt = DateTime.Now;

                // Xử lý upload ảnh mới nếu có
                if (image != null)
                {
                    DeleteFile(song.ImageUrl);
                    song.ImageUrl = await UploadFile(image, "images/songs");
                }

                // Xử lý upload audio mới nếu có
                if (audioFile != null)
                {
                    DeleteFile(song.FileUrl);
                    song.FileUrl = await UploadFile(audioFile, "audio");
                    song.Duration = GetAudioDuration(audioFile);
                    song.FileType = Path.GetExtension(audioFile.FileName);
                }

                await _songRepository.UpdateAsync(song);
                TempData["Success"] = "Cập nhật bài hát thành công";
                return RedirectToAction(nameof(Songs));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật bài hát: " + ex.Message;
                return RedirectToAction(nameof(Songs));
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSong(int id)
        {
            try
            {
                var song = await _songRepository.GetByIdAsync(id);
                if (song == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài hát" });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Cập nhật PlayHistories - set SongId thành null
                    var playHistories = await _context.PlayHistories.Where(p => p.SongId == id).ToListAsync();
                    foreach (var history in playHistories)
                    {
                        history.SongId = 0;
                    }

                    // Xóa PlaylistSongs liên quan
                    var playlistSongs = await _context.PlaylistSongs.Where(ps => ps.SongId == id).ToListAsync();
                    _context.PlaylistSongs.RemoveRange(playlistSongs);

                    // Xóa Lyrics liên quan
                    var lyrics = await _context.Lyrics.Where(l => l.SongId == id).ToListAsync();
                    _context.Lyrics.RemoveRange(lyrics);

                    // Xóa UserFavoriteSongs liên quan
                    var favoriteSongs = await _context.UserFavoriteSongs.Where(f => f.SongId == id).ToListAsync();
                    _context.UserFavoriteSongs.RemoveRange(favoriteSongs);

                    // Lưu các thay đổi
                    await _context.SaveChangesAsync();

                    // Xóa file ảnh và audio
                    DeleteFile(song.ImageUrl);
                    DeleteFile(song.FileUrl);

                    // Xóa bài hát
                    await _songRepository.DeleteAsync(id);

                    await transaction.CommitAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Có lỗi xảy ra khi xóa bài hát: " + ex.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa bài hát: " + ex.Message });
            }
        }

        private async Task<string> UploadFile(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ");

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/{folder}/{uniqueFileName}";
        }

        private void DeleteFile(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return;

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, fileUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        private TimeSpan GetAudioDuration(IFormFile audioFile)
        {
            // TODO: Implement audio duration detection
            // Tạm thời return giá trị mặc định
            return TimeSpan.FromMinutes(3);
        }

        // GET: /Admin/Playlists
        [HttpGet]
        public async Task<IActionResult> Playlists()
        {
            try
            {
                var playlists = await _playlistRepository.GetAllAsync();
                ViewBag.Users = await _userManager.Users.ToListAsync();
                return View(playlists);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách playlist: " + ex.Message;
                return View(new List<Playlist>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPlaylist(int id)
        {
            try
            {
                var playlist = await _playlistRepository.GetByIdAsync(id);
                if (playlist == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy playlist" });
                }

                var user = await _userManager.FindByIdAsync(playlist.UserId);
                var songs = playlist.PlaylistSongs?.Select(ps => ps.Song).ToList() ?? new List<Song>();

                return Json(new
                {
                    success = true,
                    id = playlist.PlaylistId,
                    title = playlist.Title,
                    description = playlist.Description ?? "",
                    imageUrl = playlist.ImageUrl,
                    isPublic = playlist.IsPublic,
                    userId = playlist.UserId,
                    userName = user?.FullName ?? "Không xác định",
                    songCount = songs.Count,
                    songs = songs.Select(s => new {
                        id = s.SongId,
                        title = s.Title,
                        artistName = s.Artists?.ArtistName ?? "Không xác định",
                        duration = s.Duration.ToString(@"mm\:ss")
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlaylist(
            string title,
            string description,
            string userId,
            IFormFile image,
            bool isPublic = true)
        {
            try
            {
                if (string.IsNullOrEmpty(title))
                {
                    TempData["Error"] = "Vui lòng nhập tên playlist";
                    return RedirectToAction(nameof(Playlists));
                }

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "Vui lòng chọn người tạo playlist";
                    return RedirectToAction(nameof(Playlists));
                }

                // Xử lý upload ảnh
                string imageUrl = null;
                if (image != null && image.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "playlists");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}_{image.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    imageUrl = $"/uploads/playlists/{uniqueFileName}";
                }

                var playlist = new Playlist
                {
                    Title = title,
                    Description = description ?? "",
                    UserId = userId,
                    ImageUrl = imageUrl,
                    IsPublic = isPublic,
                    CreatedAt = DateTime.Now,
                };

                await _playlistRepository.AddAsync(playlist);
                TempData["Success"] = "Thêm playlist thành công";
                return RedirectToAction(nameof(Playlists));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi thêm playlist: " + ex.Message;
                if (ex.InnerException != null)
                {
                    TempData["Error"] += "\nChi tiết: " + ex.InnerException.Message;
                }
                return RedirectToAction(nameof(Playlists));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditPlaylist(
            int id,
            string title,
            string description,
            bool isPublic,
            IFormFile? image)
        {
            try
            {
                var playlist = await _playlistRepository.GetByIdAsync(id);
                if (playlist == null)
                {
                    TempData["Error"] = "Không tìm thấy playlist";
                    return RedirectToAction(nameof(Playlists));
                }

                // Cập nhật thông tin cơ bản
                playlist.Title = title;
                playlist.Description = description ?? "";
                playlist.IsPublic = isPublic;
                playlist.CreatedAt = DateTime.Now;

                // Xử lý upload ảnh mới nếu có
                if (image != null)
                {
                    DeleteFile(playlist.ImageUrl);
                    playlist.ImageUrl = await UploadFile(image, "images/playlists");
                }

                await _playlistRepository.UpdateAsync(playlist);
                TempData["Success"] = "Cập nhật playlist thành công";
                return RedirectToAction(nameof(Playlists));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật playlist: " + ex.Message;
                return RedirectToAction(nameof(Playlists));
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeletePlaylist(int id)
        {
            try
            {
                var playlist = await _playlistRepository.GetByIdAsync(id);
                if (playlist == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy playlist" });
                }

                // Xóa file ảnh
                DeleteFile(playlist.ImageUrl);

                await _playlistRepository.DeleteAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa playlist: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddSongToPlaylist(int playlistId, int songId)
        {
            try
            {
                var playlist = await _playlistRepository.GetByIdAsync(playlistId);
                if (playlist == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy playlist" });
                }

                await _playlistRepository.AddSongToPlaylistAsync(playlistId, songId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm bài hát vào playlist: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSongFromPlaylist(int playlistId, int songId)
        {
            try
            {
                var playlist = await _playlistRepository.GetByIdAsync(playlistId);
                if (playlist == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy playlist" });
                }

                await _playlistRepository.RemoveSongFromPlaylistAsync(playlistId, songId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa bài hát khỏi playlist: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReorderPlaylistSongs(int playlistId, List<int> songIds)
        {
            try
            {
                var playlist = await _playlistRepository.GetByIdAsync(playlistId);
                if (playlist == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy playlist" });
                }

                await _playlistRepository.ReorderPlaylistSongsAsync(playlistId, songIds);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi sắp xếp lại bài hát: " + ex.Message });
            }
        }
    }

    // Lớp để lưu trữ thống kê theo tháng
    public class MonthlyStats
    {
        public string Month { get; set; }
        public int PlayCount { get; set; }
        public int ActiveUsers { get; set; }
        public int NewSongs { get; set; }
        public int AverageListeningTime { get; set; }
        public double Growth { get; set; }
    }

    // Lớp để lưu trữ thống kê theo ngày
    public class DailyStats
    {
        public string Date { get; set; }
        public string DayOfWeek { get; set; }
        public int PlayCount { get; set; }
        public int ActiveUsers { get; set; }
        public int NewSongs { get; set; }
        public int AverageListeningTime { get; set; }
        public double Growth { get; set; }
    }
}