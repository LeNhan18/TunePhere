using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.EntityFrameworkCore;

namespace TunePhere.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly AppDbContext _context;
        
        public UsersController(
            UserManager<AppUser> userManager, 
            IUserRepository userRepository, 
            IWebHostEnvironment hostEnvironment,
            AppDbContext context)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _hostEnvironment = hostEnvironment;
            _context = context;
        }
        
        public async Task<IActionResult> Details()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Load danh sách nghệ sĩ được theo dõi
            var followedArtists = await _context.ArtistFollowers
                .Include(af => af.Artist)
                .Where(af => af.UserId == user.Id)
                .Select(af => af.Artist)
                .ToListAsync();

            user.FollowedArtists = followedArtists;
            
            // Load lịch sử phát nhạc
            var playHistory = await _context.PlayHistories
                .Include(ph => ph.Song)
                .Include(ph => ph.Song.Artists)
                .Where(ph => ph.UserId == user.Id)
                .OrderByDescending(ph => ph.PlayedAt)
                .ToListAsync();
                
            user.PlayHistory = playHistory;
            
            return View(user);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string FullName, IFormFile ProfileImage, string CurrentImageUrl, IFormFile CoverImage, string CurrentCoverUrl)
        {
            // Lấy thông tin người dùng hiện tại
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            
            // Cập nhật tên đầy đủ
            user.FullName = FullName;
            
            // Xử lý tải lên ảnh đại diện mới
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                // Xóa ảnh cũ nếu tồn tại
                if (!string.IsNullOrEmpty(user.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, user.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                
                // Tạo tên file duy nhất
                string fileName = $"{user.Id}_profile_{DateTime.Now.Ticks}{Path.GetExtension(ProfileImage.FileName)}";
                
                // Đường dẫn lưu file
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                
                string filePath = Path.Combine(uploadsFolder, fileName);
                
                // Lưu file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(fileStream);
                }
                
                // Cập nhật URL ảnh đại diện
                user.ImageUrl = $"/images/profiles/{fileName}";
            }
            
            // Xử lý tải lên ảnh bìa mới
            if (CoverImage != null && CoverImage.Length > 0)
            {
                // Xóa ảnh bìa cũ nếu tồn tại
                if (!string.IsNullOrEmpty(user.CoverImage))
                {
                    var oldCoverPath = Path.Combine(_hostEnvironment.WebRootPath, user.CoverImage.TrimStart('/'));
                    if (System.IO.File.Exists(oldCoverPath))
                    {
                        System.IO.File.Delete(oldCoverPath);
                    }
                }
                
                // Tạo tên file duy nhất
                string fileName = $"{user.Id}_cover_{DateTime.Now.Ticks}{Path.GetExtension(CoverImage.FileName)}";
                
                // Đường dẫn lưu file
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "covers");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                
                string filePath = Path.Combine(uploadsFolder, fileName);
                
                // Lưu file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await CoverImage.CopyToAsync(fileStream);
                }
                
                // Cập nhật URL ảnh bìa
                user.CoverImage = $"/images/covers/{fileName}";
            }
            
            // Cập nhật thông tin người dùng
            await _userManager.UpdateAsync(user);
            
            // Chuyển hướng về trang hồ sơ
            return RedirectToAction("Details");
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProfile(string FullName, IFormFile ProfileImage, IFormFile CoverImage)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            
            // Cập nhật tên
            user.FullName = FullName;
            
            // Xử lý ảnh đại diện
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                
                string uniqueFileName = $"{Guid.NewGuid()}_{ProfileImage.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(fileStream);
                }
                
                user.ImageUrl = $"/images/profiles/{uniqueFileName}";
            }
            
            // Xử lý ảnh bìa
            if (CoverImage != null && CoverImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "covers");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                
                string uniqueFileName = $"{Guid.NewGuid()}_{CoverImage.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await CoverImage.CopyToAsync(fileStream);
                }
                
                user.CoverImage = $"/images/covers/{uniqueFileName}";
            }
            
            await _userManager.UpdateAsync(user);
            
            return RedirectToAction("Details");
        }
    }
} 