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
    public class ListeningRoomsController : Controller
    {
        private readonly AppDbContext _context;

        public ListeningRoomsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ListeningRooms
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ListeningRooms.Include(l => l.Creator).Include(l => l.CurrentSong);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ListeningRooms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listeningRoom = await _context.ListeningRooms
                .Include(l => l.Creator)
                .Include(l => l.CurrentSong)
                .FirstOrDefaultAsync(m => m.RoomId == id);
            if (listeningRoom == null)
            {
                return NotFound();
            }

            return View(listeningRoom);
        }

        // GET: ListeningRooms/Create
        public IActionResult Create()
        {
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["CurrentSongId"] = new SelectList(_context.Songs, "SongId", "Artist");
            return View();
        }

        // POST: ListeningRooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,CreatorId,Title,IsActive,CurrentSongId,CreatedAt")] ListeningRoom listeningRoom)
        {
            if (ModelState.IsValid)
            {
                _context.Add(listeningRoom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id", listeningRoom.CreatorId);
            ViewData["CurrentSongId"] = new SelectList(_context.Songs, "SongId", "Artist", listeningRoom.CurrentSongId);
            return View(listeningRoom);
        }

        // GET: ListeningRooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listeningRoom = await _context.ListeningRooms.FindAsync(id);
            if (listeningRoom == null)
            {
                return NotFound();
            }
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id", listeningRoom.CreatorId);
            ViewData["CurrentSongId"] = new SelectList(_context.Songs, "SongId", "Artist", listeningRoom.CurrentSongId);
            return View(listeningRoom);
        }

        // POST: ListeningRooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomId,CreatorId,Title,IsActive,CurrentSongId,CreatedAt")] ListeningRoom listeningRoom)
        {
            if (id != listeningRoom.RoomId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listeningRoom);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListeningRoomExists(listeningRoom.RoomId))
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
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id", listeningRoom.CreatorId);
            ViewData["CurrentSongId"] = new SelectList(_context.Songs, "SongId", "Artist", listeningRoom.CurrentSongId);
            return View(listeningRoom);
        }

        // GET: ListeningRooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listeningRoom = await _context.ListeningRooms
                .Include(l => l.Creator)
                .Include(l => l.CurrentSong)
                .FirstOrDefaultAsync(m => m.RoomId == id);
            if (listeningRoom == null)
            {
                return NotFound();
            }

            return View(listeningRoom);
        }

        // POST: ListeningRooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var listeningRoom = await _context.ListeningRooms.FindAsync(id);
            if (listeningRoom != null)
            {
                _context.ListeningRooms.Remove(listeningRoom);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ListeningRoomExists(int id)
        {
            return _context.ListeningRooms.Any(e => e.RoomId == id);
        }
    }
}
