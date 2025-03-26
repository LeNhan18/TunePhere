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

        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artist = await _context.Artists
                .Include(a => a.Songs)
                .Include(a => a.Albums)
                .FirstOrDefaultAsync(a => a.userId == userId);

            if (artist == null)
            {
                return NotFound();
            }

            // Lấy danh sách nghệ sĩ tương tự (cùng thể loại)
            var similarArtists = await _context.Artists
                .Where(a => a.ArtistId != artist.ArtistId)
                .OrderByDescending(a => a.Songs.Count)
                .Take(8)
                .ToListAsync();

            ViewBag.SimilarArtists = similarArtists;

            return View(artist);
        }
    }
}
