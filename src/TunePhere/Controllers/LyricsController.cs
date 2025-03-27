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
                // Thêm debug log
                Console.WriteLine($"Received lyric: SongId={lyric.SongId}, Language={lyric.Language}, Content length={lyric?.Content?.Length ?? 0}");
                
                // Kiểm tra dữ liệu thủ công
                if (lyric.SongId <= 0)
                {
                    ModelState.AddModelError("SongId", "SongId không hợp lệ");
                    Console.WriteLine("SongId không hợp lệ");
                }
                
                if (string.IsNullOrEmpty(lyric.Content))
                {
                    ModelState.AddModelError("Content", "Nội dung lời bài hát không được để trống");
                    Console.WriteLine("Content trống");
                }
                
                if (string.IsNullOrEmpty(lyric.Language))
                {
                    lyric.Language = "vi"; // Mặc định là tiếng Việt nếu không chọn
                    Console.WriteLine("Thiết lập Language = vi");
                }

                if (ModelState.IsValid)
                {
                    // Thiết lập các giá trị mặc định
                    lyric.CreatedAt = DateTime.Now;
                    
                    // Thêm lyrics vào database
                    _context.Lyrics.Add(lyric);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Đã lưu lyric với ID {lyric.LyricId}");
                    
                    // Chuyển hướng đến trang chi tiết bài hát
                    return RedirectToAction("Details", "Songs", new { id = lyric.SongId });
                }
                else
                {
                    // Log tất cả lỗi trong ModelState
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine($"Model error: {error.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                Console.WriteLine($"Lỗi khi thêm lyric: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
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

        // Thêm API endpoint này
        [HttpPost]
        [Route("api/lyrics/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLyric([FromBody] Lyric lyric)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Thiết lập giá trị mặc định
                lyric.CreatedAt = DateTime.Now;
                
                // Thêm vào database
                _context.Lyrics.Add(lyric);
                await _context.SaveChangesAsync();
                
                return Ok(new { success = true, lyricId = lyric.LyricId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Thêm phương thức này vào controller
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLyric(int SongId, string Content, string Language = "vi")
        {
            try
            {
                Console.WriteLine($"LyricsController.AddLyric - Dữ liệu nhận được: SongId={SongId}, Content length={Content?.Length ?? 0}, Language={Language}");
                
                // Kiểm tra dữ liệu đầu vào
                if (SongId <= 0)
                {
                    TempData["Error"] = "ID bài hát không hợp lệ";
                    return RedirectToAction("Details", "Songs", new { id = SongId });
                }
                
                if (string.IsNullOrEmpty(Content))
                {
                    TempData["Error"] = "Nội dung lời bài hát không được để trống";
                    return RedirectToAction("Details", "Songs", new { id = SongId });
                }

                // Kiểm tra bài hát có tồn tại không
                var song = await _context.Songs.FindAsync(SongId);
                if (song == null)
                {
                    TempData["Error"] = "Không tìm thấy bài hát";
                    return RedirectToAction("Index", "Songs");
                }

                // In nội dung lời bài hát để debug
                Console.WriteLine("Nội dung lời bài hát:");
                Console.WriteLine(Content);
                
                // Tạo đối tượng Lyric mới
                var lyric = new Lyric
                {
                    SongId = SongId,
                    Content = Content, // Đảm bảo giữ nguyên định dạng
                    Language = string.IsNullOrEmpty(Language) ? "vi" : Language,
                    CreatedAt = DateTime.Now
                };

                // Thêm vào cơ sở dữ liệu
                _context.Lyrics.Add(lyric);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"LyricsController.AddLyric - Đã lưu thành công lời bài hát với ID={lyric.LyricId}");

                // Chuyển hướng về trang details
                return RedirectToAction("Details", "Songs", new { id = SongId });
            }
            catch (Exception ex)
            {
                // Ghi log chi tiết lỗi
                Console.WriteLine($"LyricsController.AddLyric - LỖI: {ex.Message}");
                Console.WriteLine($"LyricsController.AddLyric - Stack trace: {ex.StackTrace}");
                
                // Lưu lỗi vào TempData để hiển thị
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Details", "Songs", new { id = SongId });
            }
        }
    }
}
