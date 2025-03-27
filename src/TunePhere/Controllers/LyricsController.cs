using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TunePhere.Models;

namespace TunePhere.Controllers
{
    public class LyricsController : Controller
    {
        private readonly AppDbContext _context;

        public LyricsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Lyrics
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Lyrics.Include(l => l.Song);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Lyrics/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lyric = await _context.Lyrics
                .Include(l => l.Song)
                .FirstOrDefaultAsync(m => m.LyricId == id);
            if (lyric == null)
            {
                return NotFound();
            }

            return View(lyric);
        }

        // GET: Lyrics/Create
        public IActionResult Create(int? songId)
        {
            try
            {
                if (songId.HasValue)
                {
                    // Nếu có songId được truyền vào, tự động chọn bài hát
                    var song = _context.Songs.Find(songId.Value);
                    if (song != null)
                    {
                        ViewData["SongName"] = song.Title;
                        ViewData["SongId"] = new SelectList(_context.Songs, "SongId", "Title", songId.Value);
                        
                        // Tạo đối tượng Lyric mới với SongId đã được thiết lập
                        var lyric = new Lyric
                        {
                            SongId = songId.Value,
                            Language = "vi",
                            CreatedAt = DateTime.Now,
                            Content = "" // Chỉ cần thiết lập Content, KHÔNG cần thiết lập Song
                        };
                        
                        return View(lyric);
                    }
                }
                
                // Nếu không có songId hoặc không tìm thấy bài hát
                ViewData["SongId"] = new SelectList(_context.Songs, "SongId", "Title");
                return View(new Lyric { Language = "vi", CreatedAt = DateTime.Now, Content = "" });
            }
            catch (Exception ex)
            {
                // Xử lý exception
                ViewData["Error"] = ex.Message;
                ViewData["SongId"] = new SelectList(_context.Songs, "SongId", "Title");
                return View(new Lyric { Language = "vi", CreatedAt = DateTime.Now, Content = "" });
            }
        }

        // POST: Lyrics/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SongId,Content,Language")] Lyric lyric)
        {
            try 
            {
                if (ModelState.IsValid)
                {
                    // Thiết lập các giá trị mặc định
                    lyric.CreatedAt = DateTime.Now;
                    
                    // KHÔNG CẦN thiết lập Song ở đây, Entity Framework sẽ tự động xử lý quan hệ này
                    // dựa trên SongId khi lưu vào cơ sở dữ liệu
                    
                    // Thêm lyrics vào database
                    _context.Add(lyric);
                    await _context.SaveChangesAsync();
                    
                    // Chuyển hướng đến trang chi tiết bài hát
                    return RedirectToAction("Details", "Songs", new { id = lyric.SongId });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
            }
            
            // Nếu không thành công, hiển thị lại form với dữ liệu đã nhập
            ViewData["SongId"] = new SelectList(_context.Songs, "SongId", "Title", lyric.SongId);
            return View(lyric);
        }

        // GET: Lyrics/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lyric = await _context.Lyrics.FindAsync(id);
            if (lyric == null)
            {
                return NotFound();
            }
            ViewData["SongId"] = new SelectList(_context.Songs, "SongId", "Artist", lyric.SongId);
            return View(lyric);
        }

        // POST: Lyrics/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LyricId,SongId,Content,Language,CreatedAt,UpdatedAt")] Lyric lyric)
        {
            if (id != lyric.LyricId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lyric);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LyricExists(lyric.LyricId))
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
            ViewData["SongId"] = new SelectList(_context.Songs, "SongId", "Artist", lyric.SongId);
            return View(lyric);
        }

        // GET: Lyrics/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lyric = await _context.Lyrics
                .Include(l => l.Song)
                .FirstOrDefaultAsync(m => m.LyricId == id);
            if (lyric == null)
            {
                return NotFound();
            }

            return View(lyric);
        }

        // POST: Lyrics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lyric = await _context.Lyrics.FindAsync(id);
            if (lyric != null)
            {
                _context.Lyrics.Remove(lyric);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LyricExists(int id)
        {
            return _context.Lyrics.Any(e => e.LyricId == id);
        }
    }
}
