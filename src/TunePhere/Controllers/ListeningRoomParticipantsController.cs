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
    public class ListeningRoomParticipantsController : Controller
    {
        private readonly AppDbContext _context;

        public ListeningRoomParticipantsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ListeningRoomParticipants
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ListeningRoomParticipants.Include(l => l.Room).Include(l => l.User);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ListeningRoomParticipants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listeningRoomParticipant = await _context.ListeningRoomParticipants
                .Include(l => l.Room)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (listeningRoomParticipant == null)
            {
                return NotFound();
            }

            return View(listeningRoomParticipant);
        }

        // GET: ListeningRoomParticipants/Create
        public IActionResult Create()
        {
            ViewData["RoomId"] = new SelectList(_context.ListeningRooms, "RoomId", "CreatorId");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: ListeningRoomParticipants/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RoomId,UserId,JoinedAt")] ListeningRoomParticipant listeningRoomParticipant)
        {
            if (ModelState.IsValid)
            {
                _context.Add(listeningRoomParticipant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoomId"] = new SelectList(_context.ListeningRooms, "RoomId", "CreatorId", listeningRoomParticipant.RoomId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", listeningRoomParticipant.UserId);
            return View(listeningRoomParticipant);
        }

        // GET: ListeningRoomParticipants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listeningRoomParticipant = await _context.ListeningRoomParticipants.FindAsync(id);
            if (listeningRoomParticipant == null)
            {
                return NotFound();
            }
            ViewData["RoomId"] = new SelectList(_context.ListeningRooms, "RoomId", "CreatorId", listeningRoomParticipant.RoomId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", listeningRoomParticipant.UserId);
            return View(listeningRoomParticipant);
        }

        // POST: ListeningRoomParticipants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomId,UserId,JoinedAt")] ListeningRoomParticipant listeningRoomParticipant)
        {
            if (id != listeningRoomParticipant.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listeningRoomParticipant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListeningRoomParticipantExists(listeningRoomParticipant.Id))
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
            ViewData["RoomId"] = new SelectList(_context.ListeningRooms, "RoomId", "CreatorId", listeningRoomParticipant.RoomId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", listeningRoomParticipant.UserId);
            return View(listeningRoomParticipant);
        }

        // GET: ListeningRoomParticipants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listeningRoomParticipant = await _context.ListeningRoomParticipants
                .Include(l => l.Room)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (listeningRoomParticipant == null)
            {
                return NotFound();
            }

            return View(listeningRoomParticipant);
        }

        // POST: ListeningRoomParticipants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var listeningRoomParticipant = await _context.ListeningRoomParticipants.FindAsync(id);
            if (listeningRoomParticipant != null)
            {
                _context.ListeningRoomParticipants.Remove(listeningRoomParticipant);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ListeningRoomParticipantExists(int id)
        {
            return _context.ListeningRoomParticipants.Any(e => e.Id == id);
        }
    }
}
