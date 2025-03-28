using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TunePhere.Controllers
{
    public class PlaylistSongsController : Controller
    {
        private readonly AppDbContext _context;

        public PlaylistSongsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PlaylistSongs
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PlaylistSongs.Include(p => p.AddedByUser).Include(p => p.Playlist).Include(p => p.Song);
            return View(await appDbContext.ToListAsync());
        }

        // GET: PlaylistSongs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playlistSong = await _context.PlaylistSongs
                .Include(p => p.AddedByUser)
                .Include(p => p.Playlist)
                .Include(p => p.Song)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (playlistSong == null)
            {
                return NotFound();
            }

            return View(playlistSong);
        }

        // GET: PlaylistSongs/Create
        public IActionResult Create()
        {
            ViewData["AddedByUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["PlaylistId"] = new SelectList(_context.Playlists, "PlaylistId", "Title");
            ViewData["SongId"] = new SelectList(_context.Songs, "SongId", "Artist");
            return View();
        }

        // POST: PlaylistSongs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PlaylistId,SongId,AddedByUserId,AddedAt,VoteCount")] PlaylistSong playlistSong)
        {
            if (ModelState.IsValid)
            {
                _context.Add(playlistSong);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AddedByUserId"] = new SelectList(_context.Users, "Id", "Id", playlistSong.AddedByUserId);
            ViewData["PlaylistId"] = new SelectList(_context.Playlists, "PlaylistId", "Title", playlistSong.PlaylistId);
            ViewData["SongId"] = new SelectList(_context.Songs, "SongId", "Artist", playlistSong.SongId);
            return View(playlistSong);
        }

        // GET: PlaylistSongs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playlistSong = await _context.PlaylistSongs.FindAsync(id);
            if (playlistSong == null)
            {
                return NotFound();
            }
            ViewData["AddedByUserId"] = new SelectList(_context.Users, "Id", "Id", playlistSong.AddedByUserId);
            ViewData["PlaylistId"] = new SelectList(_context.Playlists, "PlaylistId", "Title", playlistSong.PlaylistId);
            ViewData["SongId"] = new SelectList(_context.Songs, "SongId", "Artist", playlistSong.SongId);
            return View(playlistSong);
        }

        // POST: PlaylistSongs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PlaylistId,SongId,AddedByUserId,AddedAt,VoteCount")] PlaylistSong playlistSong)
        {
            if (id != playlistSong.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(playlistSong);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlaylistSongExists(playlistSong.Id))
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
            ViewData["AddedByUserId"] = new SelectList(_context.Users, "Id", "Id", playlistSong.AddedByUserId);
            ViewData["PlaylistId"] = new SelectList(_context.Playlists, "PlaylistId", "Title", playlistSong.PlaylistId);
            ViewData["SongId"] = new SelectList(_context.Songs, "SongId", "Artist", playlistSong.SongId);
            return View(playlistSong);
        }

        // GET: PlaylistSongs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playlistSong = await _context.PlaylistSongs
                .Include(p => p.AddedByUser)
                .Include(p => p.Playlist)
                .Include(p => p.Song)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (playlistSong == null)
            {
                return NotFound();
            }

            return View(playlistSong);
        }

        // POST: PlaylistSongs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var playlistSong = await _context.PlaylistSongs.FindAsync(id);
            if (playlistSong != null)
            {
                _context.PlaylistSongs.Remove(playlistSong);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: PlaylistSongs/AddSongToPlaylist
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddSongToPlaylist(int playlistId, int songId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Kiểm tra playlist có tồn tại và thuộc về user hiện tại
                var playlist = await _context.Playlists
                    .FirstOrDefaultAsync(p => p.PlaylistId == playlistId && p.UserId == userId);

                if (playlist == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy playlist hoặc bạn không có quyền thêm bài hát vào playlist này" });
                }

                // Kiểm tra bài hát đã có trong playlist chưa
                var existingSong = await _context.PlaylistSongs
                    .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

                if (existingSong != null)
                {
                    return Json(new { success = false, message = "Bài hát đã có trong playlist" });
                }

                // Thêm bài hát vào playlist
                var playlistSong = new PlaylistSong
                {
                    PlaylistId = playlistId,
                    SongId = songId,
                    AddedByUserId = userId,
                    AddedAt = DateTime.Now,
                    VoteCount = 0
                };

                _context.PlaylistSongs.Add(playlistSong);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Đã thêm bài hát vào playlist",
                    songCount = await _context.PlaylistSongs.CountAsync(ps => ps.PlaylistId == playlistId)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: PlaylistSongs/RemoveSongFromPlaylist
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveSongFromPlaylist(int playlistId, int songId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Kiểm tra playlist có tồn tại và thuộc về user hiện tại
                var playlist = await _context.Playlists
                    .FirstOrDefaultAsync(p => p.PlaylistId == playlistId && p.UserId == userId);

                if (playlist == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy playlist hoặc bạn không có quyền xóa bài hát khỏi playlist này" });
                }

                // Tìm và xóa bài hát khỏi playlist
                var playlistSong = await _context.PlaylistSongs
                    .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

                if (playlistSong == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài hát trong playlist" });
                }

                _context.PlaylistSongs.Remove(playlistSong);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Đã xóa bài hát khỏi playlist",
                    songCount = await _context.PlaylistSongs.CountAsync(ps => ps.PlaylistId == playlistId)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        private bool PlaylistSongExists(int id)
        {
            return _context.PlaylistSongs.Any(e => e.Id == id);
        }
    }
}
