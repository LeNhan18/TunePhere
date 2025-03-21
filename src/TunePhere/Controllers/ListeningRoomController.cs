using Microsoft.AspNetCore.Mvc;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Controllers
{
    public class ListeningRoomController : Controller
    {
        private readonly IListeningRoomRepository _roomRepo;

        public ListeningRoomController(IListeningRoomRepository roomRepo)
        {
            _roomRepo = roomRepo;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _roomRepo.GetActiveRoomsAsync();
            return View(rooms);
        }

        public async Task<IActionResult> Details(int id)
        {
            var room = await _roomRepo.GetByIdAsync(id);
            if (room == null) return NotFound();

            return View(room);
        }
    }
}
