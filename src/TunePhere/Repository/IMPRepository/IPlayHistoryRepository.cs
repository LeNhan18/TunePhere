using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface IPlayHistoryRepository
    {
        Task<IEnumerable<PlayHistory>> GetAllAsync();
        Task<PlayHistory?> GetByIdAsync(int id);
        Task<IEnumerable<PlayHistory>> GetBySongIdAsync(int songId);
        Task<IEnumerable<PlayHistory>> GetByUserIdAsync(string userId);
        Task<IEnumerable<PlayHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> AddAsync(PlayHistory playHistory);
        Task<bool> DeleteAsync(int id);
    }
} 