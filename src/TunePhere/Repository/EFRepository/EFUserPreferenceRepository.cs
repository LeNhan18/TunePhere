using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Repository.EFRepository
{
    public class EFUserPreferenceRepository : IUserPreferenceRepository
    {
        private readonly AppDbContext _context;

        public EFUserPreferenceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserPreference?> GetByUserIdAsync(int userId)
        {
            return await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task AddOrUpdateAsync(UserPreference preference)
        {
            var existing = await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == preference.UserId);
            if (existing == null)
            {
                _context.UserPreferences.Add(preference);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(preference);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int userId)
        {
            var preference = await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (preference != null)
            {
                _context.UserPreferences.Remove(preference);
                await _context.SaveChangesAsync();
            }
        }
    }
}
