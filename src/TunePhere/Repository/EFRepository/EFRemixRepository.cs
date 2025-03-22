using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Repository.EFRepository
{
    public class EFRemixRepository : IRemixRepository
    {
        private readonly AppDbContext _context;

        public EFRemixRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Remix>> GetAllAsync()
        {
            return await _context.Remixes.ToListAsync();
        }

        public async Task<Remix?> GetByIdAsync(int remixId)
        {
            return await _context.Remixes.FindAsync(remixId);
        }

        public async Task AddAsync(Remix remix)
        {
            _context.Remixes.Add(remix);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Remix remix)
        {
            _context.Remixes.Update(remix);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int remixId)
        {
            var remix = await _context.Remixes.FindAsync(remixId);
            if (remix != null)
            {
                _context.Remixes.Remove(remix);
                await _context.SaveChangesAsync();
            }
        }
        // L?y remix theo l??t thích gi?m d?n
        public async Task<IEnumerable<Remix>> GetTopRemixesAsync()
        {
            return await _context.Remixes
                .OrderByDescending(r => r.Likes)
                .Take(10)
                .ToListAsync();
        }
        // L?y remix c?a ng??i dùng theo username
        public async Task<IEnumerable<Remix>> GetUserRemixesAsync(string username)
        {
            return await _context.Remixes
                .Include(r => r.User)
                .Where(r => r.User.UserName == username)
                .ToListAsync();
        }
    }
}
