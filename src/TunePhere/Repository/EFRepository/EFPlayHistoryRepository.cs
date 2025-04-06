using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Repository.EFRepository
{
    public class EFPlayHistoryRepository : IPlayHistoryRepository
    {
        private readonly AppDbContext _context;

        public EFPlayHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PlayHistory>> GetAllAsync()
        {
            return await _context.PlayHistories
                .Include(p => p.Song)
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task<PlayHistory?> GetByIdAsync(int id)
        {
            return await _context.PlayHistories
                .Include(p => p.Song)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PlayHistory>> GetBySongIdAsync(int songId)
        {
            return await _context.PlayHistories
                .Include(p => p.Song)
                .Include(p => p.User)
                .Where(p => p.SongId == songId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PlayHistory>> GetByUserIdAsync(string userId)
        {
            return await _context.PlayHistories
                .Include(p => p.Song)
                .Include(p => p.User)
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PlayHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.PlayHistories
                .Include(p => p.Song)
                .Include(p => p.User)
                .Where(p => p.PlayedAt >= startDate && p.PlayedAt <= endDate)
                .ToListAsync();
        }

        public async Task<int> AddAsync(PlayHistory playHistory)
        {
            _context.PlayHistories.Add(playHistory);
            await _context.SaveChangesAsync();
            return playHistory.Id;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var playHistory = await _context.PlayHistories.FindAsync(id);
            if (playHistory == null)
                return false;

            _context.PlayHistories.Remove(playHistory);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 