using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TunePhere.Models;
using TunePhere.Repository;
using Microsoft.AspNetCore.Identity;

namespace TunePhere.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<Song> _songRepository;
        private readonly IRepository<Playlist> _playlistRepository;
        private readonly IRepository<PlayHistory> _playHistoryRepository;

        public DashboardController(
            UserManager<AppUser> userManager,
            IRepository<Song> songRepository,
            IRepository<Playlist> playlistRepository,
            IRepository<PlayHistory> playHistoryRepository)
        {
            _userManager = userManager;
            _songRepository = songRepository;
            _playlistRepository = playlistRepository;
            _playHistoryRepository = playHistoryRepository;
        }

        public IActionResult Index()
        {
            // Thống kê tổng quan
            ViewBag.TotalUsers = _userManager.Users.Count();
            ViewBag.TotalSongs = _songRepository.GetAll().Count();
            ViewBag.TotalPlaylists = _playlistRepository.GetAll().Count();
            ViewBag.TotalPlays = _playHistoryRepository.GetAll().Count();

            // Thống kê top bài hát
            var topSongs = _songRepository.GetAll()
                .OrderByDescending(s => s.PlayCount)
                .Take(5)
                .Select(s => new { Title = s.Title, PlayCount = s.PlayCount })
                .ToList();
            ViewBag.TopSongs = topSongs;

            // Thống kê lượt nghe theo tháng
            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            var playCounts = new List<int>();
            var months = new List<string>();

            foreach (var month in last6Months)
            {
                var count = _playHistoryRepository.GetAll()
                    .Count(p => p.PlayedAt.Month == month.Month && p.PlayedAt.Year == month.Year);
                playCounts.Add(count);
                months.Add(month.ToString("MM/yyyy"));
            }

            ViewBag.PlayCounts = playCounts;
            ViewBag.Months = months;

            return View();
        }
    }
} 