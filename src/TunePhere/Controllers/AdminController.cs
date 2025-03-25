using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TunePhere.Models;

namespace TunePhere.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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

        // Các action khác dành cho admin...
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }
    }
}