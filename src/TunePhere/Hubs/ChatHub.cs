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
        private static Dictionary<string, string> _userRooms = new Dictionary<string, string>();
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
                await Clients.Caller.SendAsync("ConnectionEstablished", Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in OnConnectedAsync: {ex.Message}");
            }
        }

        public async Task JoinRoom(int roomId)
        {
            try 
            {
                string roomName = $"Room_{roomId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                _userRooms[Context.ConnectionId] = roomName;
                
                string userName = Context.User?.Identity?.Name ?? "Anonymous";
                _logger.LogInformation($"User {userName} joined room {roomId}");

                // Lấy tin nhắn cũ
                var messages = await _chatRepository.GetRoomMessagesAsync(roomId);
                await Clients.Caller.SendAsync("LoadMessages", messages);
                
                // Thông báo cho tất cả người dùng trong phòng
                var systemMessage = new ChatMessage
                {
                    Content = $"{userName} đã tham gia phòng chat",
                    RoomId = roomId,
                    SenderId = Context.UserIdentifier,
                    IsSystemMessage = true
                };
                await _chatRepository.AddMessageAsync(systemMessage);
                await Clients.Group(roomName).SendAsync("UserJoined", userName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in JoinRoom: {ex.Message}");
                await Clients.Caller.SendAsync("ErrorOccurred", "Không thể tham gia phòng chat: " + ex.Message);
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

                if (!_userRooms.ContainsValue(roomName))
                {
                    throw new Exception("Bạn cần tham gia phòng để gửi tin nhắn");
                }

                // Lưu tin nhắn vào database
                var chatMessage = new ChatMessage
                {
                    Content = message,
                    RoomId = roomId,
                    SenderId = Context.UserIdentifier,
                    IsSystemMessage = false
                };
                await _chatRepository.AddMessageAsync(chatMessage);

                _logger.LogInformation($"User {userName} sending message to room {roomId}: {message}");
                
                // Gửi tin nhắn đến tất cả người dùng trong phòng
                await Clients.Group(roomName).SendAsync("ReceiveMessage", userName, message, DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SendMessage: {ex.Message}");
                await Clients.Caller.SendAsync("ErrorOccurred", "Không thể gửi tin nhắn: " + ex.Message);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                if (_userRooms.TryGetValue(Context.ConnectionId, out string? roomName))
                {
                    string userName = Context.User?.Identity?.Name ?? "Anonymous";
                    _logger.LogInformation($"User {userName} disconnected from room {roomName}");

                    // Lưu thông báo rời phòng
                    var roomId = int.Parse(roomName.Replace("Room_", ""));
                    var systemMessage = new ChatMessage
                    {
                        Content = $"{userName} đã rời phòng chat",
                        RoomId = roomId,
                        SenderId = Context.UserIdentifier,
                        IsSystemMessage = true
                    };
                    await _chatRepository.AddMessageAsync(systemMessage);
                    
                    // Thông báo cho tất cả người dùng trong phòng
                    await Clients.Group(roomName).SendAsync("UserLeft", userName);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
                    _userRooms.Remove(Context.ConnectionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in OnDisconnectedAsync: {ex.Message}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
} 