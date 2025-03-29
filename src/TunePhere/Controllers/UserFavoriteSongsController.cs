using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace TunePhere.Controllers
{
    [Authorize] // Yêu cầu đăng nhập
    public class UserFavoriteSongsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UserFavoriteSongsController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: UserFavoriteSongs
        public async Task<IActionResult> Index()
        {
            // Lấy ID của người dùng hiện tại
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge(); // Chuyển hướng đến trang đăng nhập
            }

            // Lấy danh sách bài hát yêu thích của người dùng
            var userFavorites = await _context.UserFavoriteSongs
                .Include(u => u.Song)
                .Include(u => u.Song.Artists)
                .Where(u => u.UserId == userId)
                .OrderByDescending(u => u.AddedDate)
                .ToListAsync();

            return View(userFavorites);
        }

        // GET: UserFavoriteSongs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userFavoriteSong = await _context.UserFavoriteSongs
                .Include(u => u.Song)
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userFavoriteSong == null)
            {
                return NotFound();
            }

            return View(userFavoriteSong);
        }

        // GET: UserFavoriteSongs/Create
        public IActionResult Create()
        {
            ViewData["SongId"] = new SelectList(_context.Songs, "SongId", "FileUrl");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: UserFavoriteSongs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,SongId,AddedDate")] UserFavoriteSong userFavoriteSong)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userFavoriteSong);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SongId"] = new SelectList(_context.Songs, "SongId", "FileUrl", userFavoriteSong.SongId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", userFavoriteSong.UserId);
            return View(userFavoriteSong);
        }

        // GET: UserFavoriteSongs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userFavoriteSong = await _context.UserFavoriteSongs.FindAsync(id);
            if (userFavoriteSong == null)
            {
                return NotFound();
            }
            ViewData["SongId"] = new SelectList(_context.Songs, "SongId", "FileUrl", userFavoriteSong.SongId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", userFavoriteSong.UserId);
            return View(userFavoriteSong);
        }

        // POST: UserFavoriteSongs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,SongId,AddedDate")] UserFavoriteSong userFavoriteSong)
        {
            if (id != userFavoriteSong.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userFavoriteSong);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserFavoriteSongExists(userFavoriteSong.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SongId"] = new SelectList(_context.Songs, "SongId", "FileUrl", userFavoriteSong.SongId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", userFavoriteSong.UserId);
            return View(userFavoriteSong);
        }

        // GET: UserFavoriteSongs/Remove/5
        public async Task<IActionResult> Remove(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var userFavoriteSong = await _context.UserFavoriteSongs
                .Include(u => u.Song)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (userFavoriteSong == null)
            {
                return NotFound();
            }

            return View(userFavoriteSong);
        }

        // POST: UserFavoriteSongs/Remove/5
        [HttpPost, ActionName("Remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var userFavoriteSong = await _context.UserFavoriteSongs
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (userFavoriteSong != null)
            {
                // Lấy bài hát để cập nhật số lượt thích
                var song = await _context.Songs.FindAsync(userFavoriteSong.SongId);
                if (song != null && song.LikeCount > 0)
                {
                    song.LikeCount -= 1;
                    _context.Update(song);
                }

                _context.UserFavoriteSongs.Remove(userFavoriteSong);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserFavoriteSongExists(int id)
        {
            return _context.UserFavoriteSongs.Any(e => e.Id == id);
        }
    }
}
