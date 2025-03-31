using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TunePhere.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace TunePhere.Controllers
{
    [Authorize(Roles = "Artist")]
    public class ArtistsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ArtistsController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Artists/Dashboard
        [AllowAnonymous]
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artist = await _context.Artists
                .Include(a => a.Albums)
                .Include(a => a.Songs)
                .FirstOrDefaultAsync(a => a.userId == userId);

            if (artist == null)
            {
                return NotFound();
            }

            return View(artist);
        }

        // GET: Artists
        public async Task<IActionResult> Index()
        {
            return View(await _context.Artists.ToListAsync());
        }

        // GET: Artists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artists = await _context.Artists
                .FirstOrDefaultAsync(m => m.ArtistId == id);
            if (artists == null)
            {
                return NotFound();
            }

            return View(artists);
        }

        // GET: Artists/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Artists/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ArtistId,ArtistName,ImageUrl,Bio,userId")] Artists artists)
        {
            if (ModelState.IsValid)
            {
                _context.Add(artists);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(artists);
        }

        // GET: Artists/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artist = await _context.Artists
                .FirstOrDefaultAsync(a => a.userId == userId);

            if (artist == null)
            {
                return NotFound();
            }

            return View(artist);
        }

        // POST: Artists/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Artists model, IFormFile? ImageFile = null, IFormFile? CoverImageFile = null)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    ModelState.AddModelError("", error);
                }
                return View(model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var artist = await _context.Artists.FindAsync(model.ArtistId);

                if (artist == null || artist.userId != userId)
                {
                    return NotFound();
                }

                // Cập nhật thông tin cơ bản
                artist.ArtistName = model.ArtistName;
                artist.Bio = model.Bio;
                artist.userId = userId; // Đảm bảo userId được gán

                // Xử lý upload ảnh đại diện
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "artists");
                    Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(artist.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", artist.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    artist.ImageUrl = "/uploads/artists/" + uniqueFileName;
                }

                // Xử lý upload ảnh bìa
                if (CoverImageFile != null && CoverImageFile.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + CoverImageFile.FileName;
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers");
                    Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await CoverImageFile.CopyToAsync(stream);
                    }

                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(artist.CoverImageUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", artist.CoverImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    artist.CoverImageUrl = "/uploads/covers/" + uniqueFileName;
                }

                _context.Update(artist);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật thông tin: " + ex.Message);
                return View(model);
            }
        }

        // GET: Artists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artists = await _context.Artists
                .FirstOrDefaultAsync(m => m.ArtistId == id);
            if (artists == null)
            {
                return NotFound();
            }

            return View(artists);
        }

        // POST: Artists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artists = await _context.Artists.FindAsync(id);
            if (artists != null)
            {
                _context.Artists.Remove(artists);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArtistsExists(int id)
        {
            return _context.Artists.Any(e => e.ArtistId == id);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Profile(int? id = null)
        {
            Artists artist;
            if (id == null)
            {
                // Nếu không có id, lấy profile của artist đang đăng nhập
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                artist = await _context.Artists
                    .Include(a => a.Songs)
                    .Include(a => a.Albums)
                    .Include(a => a.Followers)
                    .FirstOrDefaultAsync(a => a.userId == userId);
            }
            else
            {
                // Lấy profile của artist theo id được truyền vào
                artist = await _context.Artists
                    .Include(a => a.Songs)
                    .Include(a => a.Albums)
                    .Include(a => a.Followers)
                    .FirstOrDefaultAsync(a => a.ArtistId == id);
            }

            if (artist == null)
            {
                return NotFound();
            }

            // Lấy danh sách nghệ sĩ tương tự và tính tổng lượt nghe
            var similarArtistsQuery = await _context.Artists
                .Include(a => a.Songs)
                .Where(a => a.ArtistId != artist.ArtistId)
                .Select(a => new
                {
                    Artist = a,
                    PlayCount = a.Songs.Sum(s => s.PlayCount)
                })
                .OrderByDescending(x => x.PlayCount)
                .Take(4)
                .ToListAsync();

            var similarArtists = similarArtistsQuery.Select(x => x.Artist).ToList();
            var artistPlayCounts = similarArtistsQuery.ToDictionary(x => x.Artist.ArtistId, x => x.PlayCount);

            ViewBag.SimilarArtists = similarArtists;
            ViewBag.ArtistPlayCounts = artistPlayCounts;

            return View(artist);
        }

        // POST: Artists/ToggleFollow
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ToggleFollow(int artistId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này" });
                }

                var artist = await _context.Artists
                    .Include(a => a.Followers)
                    .FirstOrDefaultAsync(a => a.ArtistId == artistId);

                if (artist == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nghệ sĩ" });
                }

                if (artist.userId == userId)
                {
                    return Json(new { success = false, message = "Bạn không thể theo dõi chính mình" });
                }

                var existingFollow = await _context.ArtistFollowers
                    .FirstOrDefaultAsync(f => f.ArtistId == artistId && f.UserId == userId);

                if (existingFollow != null)
                {
                    _context.ArtistFollowers.Remove(existingFollow);
                }
                else
                {
                    var follow = new ArtistFollower
                    {
                        ArtistId = artistId,
                        UserId = userId,
                        FollowedAt = DateTime.Now
                    };
                    _context.ArtistFollowers.Add(follow);
                }

                await _context.SaveChangesAsync();

                // Refresh artist data to get updated follower count
                artist = await _context.Artists
                    .Include(a => a.Followers)
                    .FirstOrDefaultAsync(a => a.ArtistId == artistId);

                return Json(new { 
                    success = true, 
                    isFollowing = existingFollow == null,
                    followersCount = artist.GetFollowersCount(),
                    message = existingFollow == null ? "Đã theo dõi nghệ sĩ" : "Đã hủy theo dõi nghệ sĩ"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Followers(int id)
        {
            var artist = await _context.Artists
                .Include(a => a.Followers)
                    .ThenInclude(f => f.User)
                .FirstOrDefaultAsync(a => a.ArtistId == id);

            if (artist == null)
            {
                return NotFound();
            }

            return View(artist);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ToggleFollowUser(string userId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này" });
                }

                if (currentUserId == userId)
                {
                    return Json(new { success = false, message = "Bạn không thể theo dõi chính mình" });
                }

                var existingFollow = await _context.UserFollowers
                    .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == userId);

                if (existingFollow != null)
                {
                    _context.UserFollowers.Remove(existingFollow);
                }
                else
                {
                    var follow = new UserFollower
                    {
                        FollowerId = currentUserId,
                        FollowingId = userId,
                        FollowedAt = DateTime.Now
                    };
                    _context.UserFollowers.Add(follow);
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    isFollowing = existingFollow == null,
                    message = existingFollow == null ? "Đã theo dõi người dùng" : "Đã hủy theo dõi người dùng"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Following(string userId = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu không có userId, lấy danh sách theo dõi của người đang đăng nhập
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }
            }

            // Lấy thông tin nghệ sĩ để hiển thị tên
            var artist = await _context.Artists
                .FirstOrDefaultAsync(a => a.userId == userId);

            ViewBag.DisplayName = artist?.ArtistName ?? "Người dùng";
            ViewBag.UserId = userId;

            // Lấy danh sách nghệ sĩ đang theo dõi
            var artistFollowing = await _context.ArtistFollowers
                .Include(f => f.Artist)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.FollowedAt)
                .Select(f => new UserFollower
                {
                    FollowerId = userId,
                    FollowingId = f.Artist.userId,
                    FollowedAt = f.FollowedAt,
                    Following = new AppUser
                    {
                        Id = f.Artist.userId,
                        FullName = f.Artist.ArtistName,
                        ImageUrl = f.Artist.ImageUrl
                    }
                })
                .ToListAsync();

            // Lấy danh sách người dùng đang theo dõi
            var userFollowing = await _context.UserFollowers
                .Include(f => f.Following)
                .Where(f => f.FollowerId == userId)
                .OrderByDescending(f => f.FollowedAt)
                .ToListAsync();

            // Tạo map giữa userId và ArtistId
            var artistMap = await _context.Artists
                .Where(a => artistFollowing.Select(f => f.FollowingId).Contains(a.userId))
                .ToDictionaryAsync(a => a.userId, a => a);

            ViewBag.ArtistMap = artistMap;

            // Kết hợp cả 2 danh sách
            var allFollowing = artistFollowing.Concat(userFollowing)
                .OrderByDescending(f => f.FollowedAt)
                .ToList();

            return View(allFollowing);
        }
    }
}
