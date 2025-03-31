using TunePhere.Models;

namespace TunePhere.Repository
{
    public interface IChatMessageRepository
    {
        Task<IEnumerable<ChatMessage>> GetRoomMessagesAsync(int roomId, int count = 50);
        Task<ChatMessage> AddMessageAsync(ChatMessage message);
        Task<bool> DeleteRoomMessagesAsync(int roomId);
    }
} 