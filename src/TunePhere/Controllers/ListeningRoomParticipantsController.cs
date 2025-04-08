using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Controllers
{
    public class ListeningRoomParticipantsController : Controller
    {
        private readonly IListeningRoomParticipantRepository _participantRepository;
        private readonly IListeningRoomRepository _roomRepository;
        private readonly IUserRepository _userRepository;
        private readonly IWebHostEnvironment _environment;

        public ListeningRoomParticipantsController(
            IListeningRoomParticipantRepository participantRepository,
            IListeningRoomRepository roomRepository,
            IUserRepository userRepository)
        {
            _participantRepository = participantRepository;
            _roomRepository = roomRepository;
            _userRepository = userRepository;
        }

        // GET: ListeningRoomParticipants
        public async Task<IActionResult> Index()
        {
            var participants = await _participantRepository.GetAllAsync();
            return View(participants);
        }

        // GET: ListeningRoomParticipants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participant = await _participantRepository.GetByIdAsync(id.Value);
            if (participant == null)
            {
                return NotFound();
            }

            return View(participant);
        }

        // GET: ListeningRoomParticipants/Create
        public async Task<IActionResult> Create()
        {
            var rooms = await _roomRepository.GetAllAsync();
            var users = await _userRepository.GetAllAsync();

            ViewBag.RoomId = new SelectList(rooms, "RoomId", "Title");
            ViewBag.UserId = new SelectList(users, "Id", "UserName");

            return View();
        }

        // POST: ListeningRoomParticipants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,UserId")] ListeningRoomParticipant participant)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem người dùng đã tham gia phòng chưa
                var existingParticipant = await _participantRepository.GetByIdsAsync(participant.RoomId, participant.UserId);
                if (existingParticipant != null)
                {
                    ModelState.AddModelError("", "Người dùng đã tham gia phòng này.");
                    return View(participant);
                }

                await _participantRepository.AddAsync(participant);
                return RedirectToAction(nameof(Index));
            }

            // Nếu có lỗi, load lại danh sách cho dropdown
            var rooms = await _roomRepository.GetAllAsync();
            var users = await _userRepository.GetAllAsync();

            ViewBag.RoomId = new SelectList(rooms, "RoomId", "Title", participant.RoomId);
            ViewBag.UserId = new SelectList(users, "Id", "UserName", participant.UserId);

            return View(participant);
        }

        // GET: ListeningRoomParticipants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participant = await _participantRepository.GetByIdAsync(id.Value);
            if (participant == null)
            {
                return NotFound();
            }

            var rooms = await _roomRepository.GetAllAsync();
            var users = await _userRepository.GetAllAsync();

            ViewBag.RoomId = new SelectList(rooms, "RoomId", "Title", participant.RoomId);
            ViewBag.UserId = new SelectList(users, "Id", "UserName", participant.UserId);

            return View(participant);
        }

        // POST: ListeningRoomParticipants/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomId,UserId")] ListeningRoomParticipant participant)
        {
            if (id != participant.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _participantRepository.UpdateAsync(participant);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _participantRepository.ExistsAsync(participant.Id))
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

            var rooms = await _roomRepository.GetAllAsync();
            var users = await _userRepository.GetAllAsync();

            ViewBag.RoomId = new SelectList(rooms, "RoomId", "Title", participant.RoomId);
            ViewBag.UserId = new SelectList(users, "Id", "UserName", participant.UserId);

            return View(participant);
        }


        // POST: ListeningRoomParticipants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var participant = await _participantRepository.GetByIdAsync(id);
            if (participant != null)
            {
                await _participantRepository.DeleteAsync(participant);
            }

            return RedirectToAction(nameof(Index));
        }

        public ListeningRoomParticipantsController(
            IListeningRoomParticipantRepository participantRepository,
            IListeningRoomRepository roomRepository,
            IUserRepository userRepository,
            IWebHostEnvironment environment)
        {
            _participantRepository = participantRepository;
            _roomRepository = roomRepository;
            _userRepository = userRepository;
            _environment = environment;
        }
    }
}
