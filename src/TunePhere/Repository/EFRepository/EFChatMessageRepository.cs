using Microsoft.EntityFrameworkCore;
using TunePhere.Models;

namespace TunePhere.Repository.EFRepository
{
    public class EFChatMessageRepository : IChatMessageRepository
    {
        private readonly AppDbContext _context;

        public EFChatMessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ChatMessage>> GetRoomMessagesAsync(int roomId, int count = 50)
        {
            return await _context.ChatMessages
                .Where(m => m.RoomId == roomId)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .Take(count)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
        {
            message.SentAt = DateTime.Now;
            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<bool> DeleteRoomMessagesAsync(int roomId)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.RoomId == roomId)
                .ToListAsync();

            if (messages.Any())
            {
                _context.ChatMessages.RemoveRange(messages);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
} 