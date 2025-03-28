using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Antiforgery;
using TunePhere.Models;
using System;
using System.Threading.Tasks;

namespace TunePhere.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class LyricsApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAntiforgery _antiforgery;

        public LyricsApiController(AppDbContext context, IAntiforgery antiforgery)
        {
            _context = context;
            _antiforgery = antiforgery;
        }

        public class LyricDto
        {
            public int SongId { get; set; }
            public string Content { get; set; }
            public string Language { get; set; } = "vi";
        }

        [HttpPost("AddLyric")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLyric([FromBody] LyricDto lyricDto)
        {
            try
            {
                if (string.IsNullOrEmpty(lyricDto.Content))
                {
                    return BadRequest("Nội dung lời bài hát không được để trống");
                }

                if (lyricDto.SongId <= 0)
                {
                    return BadRequest("ID bài hát không hợp lệ");
                }

                // Kiểm tra bài hát có tồn tại không
                var song = await _context.Songs.FindAsync(lyricDto.SongId);
                if (song == null)
                {
                    return NotFound("Không tìm thấy bài hát");
                }

                // Tạo mới lyric
                var lyric = new Lyric
                {
                    SongId = lyricDto.SongId,
                    Content = lyricDto.Content,
                    Language = lyricDto.Language ?? "vi",
                    CreatedAt = DateTime.Now
                };

                _context.Lyrics.Add(lyric);
                await _context.SaveChangesAsync();

                return Ok(new { id = lyric.LyricId, message = "Đã thêm lời bài hát thành công" });
            }
            catch (Exception ex)
            {
                // Log lỗi (thực tế nên dùng Logger)
                Console.WriteLine($"Lỗi khi thêm lyric: {ex.Message}");
                return StatusCode(500, "Đã xảy ra lỗi khi lưu lời bài hát");
            }
        }
    }
}  