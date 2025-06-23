using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TunePhere.Models;

namespace TunePhere.Controllers
{
    [Authorize]
    public class ArtistRegistrationController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public ArtistRegistrationController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: ArtistRegistration
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("ArtistName,Bio")] Artists artist, IFormFile? ArtistImage, IFormFile? CoverImage)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng.");
            }

            // Kiểm tra trạng thái thanh toán
            if (!user.IsArtist)
            {
                TempData["ErrorMessage"] = "Bạn cần thanh toán phí đăng ký nghệ sĩ trước!";
                return RedirectToAction("ArtistRegistrationPayment", "Payment");
            }

            // Kiểm tra xem user đã là nghệ sĩ chưa
            var existingArtist = await _context.Artists.FirstOrDefaultAsync(a => a.userId == user.Id);
            if (existingArtist != null)
            {
                ModelState.AddModelError("", "Bạn đã là nghệ sĩ rồi!");
                return View(artist);
            }

            // Đảm bảo role Artist tồn tại
            if (!await _roleManager.RoleExistsAsync("Artist"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Artist"));
            }

            // Gán userId cho artist
            artist.userId = user.Id;

            // Xử lý upload ảnh đại diện
            if (ArtistImage != null && ArtistImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "artists");
                Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa tồn tại

                var uniqueFileName = $"{Guid.NewGuid()}_{ArtistImage.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ArtistImage.CopyToAsync(fileStream);
                }

                artist.ImageUrl = $"/uploads/artists/{uniqueFileName}";
            }

            // Xử lý upload ảnh bìa
            if (CoverImage != null && CoverImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "artists");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{CoverImage.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await CoverImage.CopyToAsync(fileStream);
                }

                artist.CoverImageUrl = $"/uploads/artists/{uniqueFileName}";
            }

            try
            {
                // Thêm role Artist cho user
                var roleResult = await _userManager.AddToRoleAsync(user, "Artist");
                if (!roleResult.Succeeded)
                {
                    ModelState.AddModelError("", "Không thể thêm quyền nghệ sĩ.");
                    return View(artist);
                }

                // Lưu thông tin nghệ sĩ
                _context.Add(artist);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Đăng ký nghệ sĩ thành công!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi đăng ký: {ex.Message}");
                return View(artist);
            }
        }
    }
}