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

                foreach (var month in last6Months)
                {
                    // Tính tổng lượt nghe của các bài hát được upload trong tháng
                    var monthlyPlays = songsList
                        .Where(s => s.UploadDate.Month == month.Month && s.UploadDate.Year == month.Year)
                        .Sum(s => s.PlayCount);
                        
                    playCounts.Add(monthlyPlays);
                    months.Add(month.ToString("MM/yyyy"));
                }

                ViewBag.PlayCounts = playCounts;
                ViewBag.Months = months;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Có lỗi xảy ra khi tải dữ liệu dashboard: {ex.Message}";
                return View();
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
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }
    }
}