using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;
using Microsoft.AspNetCore.Authorization;

namespace TunePhere.Controllers
{
    [Authorize]
    public class ListeningRoomsController : Controller
    {
        private readonly IListeningRoomRepository _roomRepository;
        private readonly ISongRepository _songRepository;
        private readonly IListeningRoomParticipantRepository _participantRepository;
        private readonly AppDbContext _context;

        public ListeningRoomsController(
            IListeningRoomRepository roomRepository,
            ISongRepository songRepository,
            IListeningRoomParticipantRepository participantRepository,
            AppDbContext context)
        {
            _roomRepository = roomRepository;
            _songRepository = songRepository;
            _participantRepository = participantRepository;
            _context = context;
        }

        // GET: ListeningRooms/SearchSongs
        [HttpGet]
        public async Task<IActionResult> SearchSongs(string searchTerm)
        {
            var songs = await _songRepository.SearchSongsAsync(searchTerm);
            return Json(songs.Select(s => new
            {
                id = s.SongId,
                text = $"{s.Title} - {(s.Artists != null ? s.Artists.ArtistName : "Unknown Artist")}"
            }));
        }

        // GET: ListeningRooms
        public async Task<IActionResult> Index()
        {
            var rooms = await _roomRepository.GetAllAsync();
            return View(rooms);
        }

        // GET: ListeningRooms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _roomRepository.GetByIdAsync(id.Value);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // GET: ListeningRooms/Create
        public async Task<IActionResult> Create()
        {
            var songs = await _songRepository.GetActiveSongsAsync();
            ViewBag.CurrentSongId = new SelectList(songs, "SongId", "Title");
            return View(new ListeningRoom { CreatorId = User.FindFirstValue(ClaimTypes.NameIdentifier) });
        }

        // POST: ListeningRooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,IsActive,CurrentSongId,CreatorId")] ListeningRoom room)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    room.CreatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    room.CreatedAt = DateTime.Now;
                    
                    await _roomRepository.AddAsync(room);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo phòng: " + ex.Message);
            }

            // Nếu có lỗi, load lại danh sách bài hát
            var activeSongs = await _songRepository.GetActiveSongsAsync();
            ViewBag.CurrentSongId = new SelectList(activeSongs, "SongId", "Title", room.CurrentSongId);
            return View(room);
        }

        // GET: ListeningRooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _roomRepository.GetByIdAsync(id.Value);
            if (room == null)
            {
                return NotFound();
            }

            var songs = await _songRepository.GetActiveSongsAsync();
            ViewBag.CurrentSongId = new SelectList(songs, "SongId", "Title", room.CurrentSongId);
            return View(room);
        }

        // POST: ListeningRooms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomId,Title,IsActive,CurrentSongId")] ListeningRoom room)
        {
            if (id != room.RoomId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin phòng hiện tại từ database
                    var existingRoom = await _roomRepository.GetByIdAsync(id);
                    if (existingRoom == null)
                    {
                        return NotFound();
                    }

                    // Giữ lại CreatorId và CreatedAt
                    room.CreatorId = existingRoom.CreatorId;
                    room.CreatedAt = existingRoom.CreatedAt;

                    await _roomRepository.UpdateAsync(room);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _roomRepository.ExistsAsync(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var songs = await _songRepository.GetActiveSongsAsync();
            ViewBag.CurrentSongId = new SelectList(songs, "SongId", "Title", room.CurrentSongId);
            return View(room);
        }

        // GET: ListeningRooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _roomRepository.GetByIdAsync(id.Value);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: ListeningRooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _roomRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: ListeningRooms/Join/5
        public async Task<IActionResult> Join(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["Error"] = "Mã phòng không hợp lệ.";
                    return RedirectToAction(nameof(Index));
                }

                var listeningRoom = await _roomRepository.GetByIdAsync(id.Value);

                if (listeningRoom == null)
                {
                    TempData["Error"] = "Không tìm thấy phòng.";
                    return RedirectToAction(nameof(Index));
                }

                if (!listeningRoom.IsActive)
                {
                    TempData["Error"] = "Phòng này hiện không hoạt động.";
                    return RedirectToAction(nameof(Index));
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "Vui lòng đăng nhập để tham gia phòng.";
                    return RedirectToAction("Login", "Account");
                }

                // Kiểm tra xem người dùng đã tham gia chưa
                var existingParticipant = await _participantRepository.GetByIdsAsync(id.Value, userId);

                if (existingParticipant != null)
                {
                    TempData["Info"] = "Bạn đã tham gia phòng này rồi.";
                    return RedirectToAction(nameof(Details), new { id = listeningRoom.RoomId });
                }

                // Thêm người dùng vào phòng
                var participant = new ListeningRoomParticipant
                {
                    RoomId = listeningRoom.RoomId,
                    UserId = userId,
                    JoinedAt = DateTime.Now,
                    Room = listeningRoom,
                    User = await _context.Users.FindAsync(userId)
                };

                await _participantRepository.AddAsync(participant);

                TempData["Success"] = "Tham gia phòng thành công!";
                return RedirectToAction(nameof(Details), new { id = listeningRoom.RoomId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tham gia phòng: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Leave(int id)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var participant = await _participantRepository.GetParticipantAsync(id, userId);
            if (participant == null)
            {
                return NotFound();
            }

            await _participantRepository.DeleteAsync(participant);
            return Ok();
        }
    }
}
