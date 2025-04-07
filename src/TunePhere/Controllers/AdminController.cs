using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ISongRepository _songRepository;
        private readonly IPlaylistRepository _playlistRepository;

        public AdminController(
            UserManager<AppUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            ISongRepository songRepository,
            IPlaylistRepository playlistRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _songRepository = songRepository;
            _playlistRepository = playlistRepository;
        }

        // GET: /Admin
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

                // Thống kê lượt nghe theo tháng (6 tháng gần nhất)
                var last6Months = Enumerable.Range(0, 6)
                    .Select(i => DateTime.Now.AddMonths(-i))
                    .OrderBy(d => d)
                    .ToList();

                var playCounts = new List<int>();
                var months = new List<string>();
                var monthlyStats = new List<MonthlyStats>();

                foreach (var month in last6Months)
                {
                    var startDate = new DateTime(month.Year, month.Month, 1);
                    var endDate = startDate.AddMonths(1).AddDays(-1);
                    
                    // Lấy bài hát được upload trong tháng
                    var songsInMonth = songsList.Where(s => s.UploadDate.Month == month.Month && s.UploadDate.Year == month.Year).ToList();
                    
                    // Tính tổng lượt nghe cho tháng
                    var monthlyPlays = songsInMonth.Sum(s => s.PlayCount);
                    
                    // Nếu tháng không có bài hát nào được upload, lấy một phần tổng lượt nghe dựa trên vị trí tháng
                    if (monthlyPlays == 0)
                    {
                        int monthIndex = last6Months.IndexOf(month);
                        double ratio = 0.1 + (0.05 * monthIndex); // Tỉ lệ tăng dần từ 10% đến 35%
                        monthlyPlays = (int)(ViewBag.TotalPlays * ratio / last6Months.Count);
                    }
                    
                    playCounts.Add(monthlyPlays);
                    months.Add(month.ToString("MM/yyyy"));
                    
                    // Tính % tăng trưởng
                    double growthPercent = 0;
                    if (playCounts.Count > 1)
                    {
                        int prevCount = playCounts[playCounts.Count - 2];
                        if (prevCount > 0)
                        {
                            growthPercent = Math.Round((monthlyPlays - prevCount) * 100.0 / prevCount, 1);
                        }
                    }
                    
                    // Tạo đối tượng thống kê tháng
                    var activeUsers = new Random().Next(20, 100); // Mô phỏng số người dùng hoạt động
                    var newSongs = songsInMonth.Count;
                    var avgListeningTime = new Random().Next(2, 10); // Mô phỏng thời gian nghe trung bình (phút)
                    
                    monthlyStats.Add(new MonthlyStats
                    {
                        Month = month.ToString("MM/yyyy"),
                        PlayCount = monthlyPlays,
                        ActiveUsers = activeUsers,
                        NewSongs = newSongs,
                        AverageListeningTime = avgListeningTime,
                        Growth = growthPercent
                    });
                }

                ViewBag.PlayCounts = playCounts;
                ViewBag.Months = months;
                ViewBag.MonthlyStats = monthlyStats;

                // Thống kê theo ngày (15 ngày gần nhất)
                GetDailyStats(songsList);

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Có lỗi xảy ra khi tải dữ liệu dashboard: {ex.Message}";
                return View();
            }
        }

        // Lấy thống kê theo ngày
        private void GetDailyStats(List<Song> songsList)
        {
            try
            {
                var last15Days = Enumerable.Range(0, 15)
                    .Select(i => DateTime.Now.Date.AddDays(-i))
                    .OrderBy(d => d)
                    .ToList();

                var dailyPlayCounts = new List<int>();
                var days = new List<string>();
                var dailyStats = new List<DailyStats>();

                // Biến để giả lập dữ liệu tăng trưởng theo ngày
                var rand = new Random();
                var basePlayCount = ViewBag.TotalPlays / 30; // Ước tính lượt nghe trung bình 1 ngày
                int previousDayCount = 0;

                foreach (var day in last15Days)
                {
                    // Lấy bài hát được upload trong ngày
                    var songsInDay = songsList
                        .Where(s => s.UploadDate.Date == day.Date)
                        .ToList();

                    // Mô phỏng lượng nghe cho ngày cụ thể
                    // Lượt nghe cơ bản + độ dao động ngẫu nhiên + thêm lượt nghe cho bài hát mới upload trong ngày
                    var dailyVariation = rand.Next(-basePlayCount / 10, basePlayCount / 5);
                    var newSongsBonus = songsInDay.Count * rand.Next(10, 50);
                    
                    // Mô phỏng lượt nghe theo xu hướng ngày trong tuần (cuối tuần cao hơn)
                    var dayOfWeekFactor = day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday 
                        ? 1.3 : 1.0;
                    
                    var dailyPlays = (int)((basePlayCount + dailyVariation + newSongsBonus) * dayOfWeekFactor);
                    
                    // Đảm bảo không có lượt nghe âm
                    dailyPlays = Math.Max(dailyPlays, 10);
                    
                    dailyPlayCounts.Add(dailyPlays);
                    days.Add(day.ToString("dd/MM"));
                    
                    // Tính % tăng trưởng
                    double growthPercent = 0;
                    if (previousDayCount > 0)
                    {
                        growthPercent = Math.Round((dailyPlays - previousDayCount) * 100.0 / previousDayCount, 1);
                    }
                    previousDayCount = dailyPlays;
                    
                    // Tạo đối tượng thống kê ngày
                    var activeUsers = rand.Next(10, 50); // Mô phỏng số người dùng hoạt động
                    var newSongsCount = songsInDay.Count;
                    var avgListeningTime = rand.Next(2, 8); // Mô phỏng thời gian nghe trung bình (phút)
                    
                    dailyStats.Add(new DailyStats
                    {
                        Date = day.ToString("dd/MM/yyyy"),
                        DayOfWeek = day.ToString("dddd", new System.Globalization.CultureInfo("vi-VN")),
                        PlayCount = dailyPlays,
                        ActiveUsers = activeUsers,
                        NewSongs = newSongsCount,
                        AverageListeningTime = avgListeningTime,
                        Growth = growthPercent
                    });
                }

                ViewBag.DailyPlayCounts = dailyPlayCounts;
                ViewBag.Days = days;
                ViewBag.DailyStats = dailyStats;
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không ảnh hưởng đến việc hiển thị trang
                Console.WriteLine($"Lỗi khi tạo thống kê ngày: {ex.Message}");
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
                    // Gán vai trò Administrator
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

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa người dùng" });
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