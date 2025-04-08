using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TunePhere.Models;

public class PlayHistoriesController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public PlayHistoriesController(AppDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var playHistory = await _context.PlayHistories
            .Include(p => p.Song)
                .ThenInclude(s => s.Artists)
            .Where(p => p.UserId == currentUser.Id)
            .OrderByDescending(p => p.PlayedAt)
            .ToListAsync();
        return View(playHistory);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddToHistory([FromBody] PlayHistoryViewModel model)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            // Kiểm tra xem bài hát có tồn tại không
            var song = await _context.Songs.FindAsync(model.songId);
            if (song == null)
                return NotFound();

            // Xóa tất cả bản ghi cũ
            await _context.PlayHistories
                .Where(p => p.SongId == model.songId && p.UserId == currentUser.Id)
                .ExecuteDeleteAsync();

            // Tạo bản ghi mới
            var playHistory = new PlayHistory
            {
                SongId = model.songId,
                UserId = currentUser.Id,
                PlayedAt = DateTime.Now
            };

            _context.PlayHistories.Add(playHistory);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return Json(new { success = true, message = "Đã cập nhật lịch sử nghe" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Json(new { success = false, message = ex.Message });
        }
    }

    public class PlayHistoryViewModel
    {
        public int songId { get; set; }
    }
}