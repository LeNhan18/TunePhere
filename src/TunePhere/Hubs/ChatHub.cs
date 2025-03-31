using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using TunePhere.Repository;
using TunePhere.Models;

namespace TunePhere.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly IChatMessageRepository _chatRepository;

        public ChatHub(ILogger<ChatHub> logger, IChatMessageRepository chatRepository)
        {
            _logger = logger;
            _chatRepository = chatRepository;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                await base.OnConnectedAsync();
                _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in OnConnectedAsync: {ex.Message}");
                throw;
            }
        }

        public async Task JoinRoom(int roomId)
        {
            try 
            {
                string roomName = $"Room_{roomId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                
                string userName = Context.User?.Identity?.Name ?? "Anonymous";
                _logger.LogInformation($"User {userName} joined room {roomId}");

                // Lấy tin nhắn cũ
                var messages = await _chatRepository.GetRoomMessagesAsync(roomId);
                await Clients.Caller.SendAsync("LoadMessages", messages);
                
                // Thông báo cho tất cả người dùng trong phòng
                await Clients.Group(roomName).SendAsync("UserJoined", userName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in JoinRoom: {ex.Message}");
                await Clients.Caller.SendAsync("ErrorOccurred", "Không thể tham gia phòng chat: " + ex.Message);
                throw;
            }
        }

        public async Task SendMessage(int roomId, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    throw new Exception("Tin nhắn không được để trống");
                }

                string roomName = $"Room_{roomId}";
                string userName = Context.User?.Identity?.Name;

                if (string.IsNullOrEmpty(userName))
                {
                    throw new Exception("Bạn cần đăng nhập để gửi tin nhắn");
                }

                // Lưu tin nhắn vào database
                var chatMessage = new ChatMessage
                {
                    Content = message,
                    RoomId = roomId,
                    SenderId = Context.UserIdentifier,
                    SentAt = DateTime.Now,
                    IsSystemMessage = false
                };
                await _chatRepository.AddMessageAsync(chatMessage);

                _logger.LogInformation($"User {userName} sending message to room {roomId}: {message}");
                
                // Gửi tin nhắn đến tất cả người dùng trong phòng
                await Clients.Group(roomName).SendAsync("ReceiveMessage", userName, message, chatMessage.SentAt);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SendMessage: {ex.Message}");
                await Clients.Caller.SendAsync("ErrorOccurred", "Không thể gửi tin nhắn: " + ex.Message);
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                await base.OnDisconnectedAsync(exception);
                _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in OnDisconnectedAsync: {ex.Message}");
                throw;
            }
        }
    }
} 