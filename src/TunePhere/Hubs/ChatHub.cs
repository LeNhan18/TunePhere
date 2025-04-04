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

                // L?y tin nh?n c?
                var messages = await _chatRepository.GetRoomMessagesAsync(roomId);
                await Clients.Caller.SendAsync("LoadMessages", messages);

                // Thông báo cho t?t c? ngý?i dùng trong ph?ng
                var systemMessage = new ChatMessage
                {
                    Content = $"{userName} ð? tham gia ph?ng chat",
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
                await Clients.Caller.SendAsync("ErrorOccurred", "Không th? tham gia ph?ng chat: " + ex.Message);
            }
        }

        public async Task SendMessage(int roomId, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    throw new Exception("Tin nh?n không ðý?c ð? tr?ng");
                }

                string roomName = $"Room_{roomId}";
                string userName = Context.User?.Identity?.Name;

                if (string.IsNullOrEmpty(userName))
                {
                    throw new Exception("B?n c?n ðãng nh?p ð? g?i tin nh?n");
                }

                if (!_userRooms.ContainsValue(roomName))
                {
                    throw new Exception("B?n c?n tham gia ph?ng ð? g?i tin nh?n");
                }

                // Lýu tin nh?n vào database
                var chatMessage = new ChatMessage
                {
                    Content = message,
                    RoomId = roomId,
                    SenderId = Context.UserIdentifier,
                    IsSystemMessage = false
                };
                await _chatRepository.AddMessageAsync(chatMessage);

                _logger.LogInformation($"User {userName} sending message to room {roomId}: {message}");

                // G?i tin nh?n ð?n t?t c? ngý?i dùng trong ph?ng
                await Clients.Group(roomName).SendAsync("ReceiveMessage", userName, message, DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SendMessage: {ex.Message}");
                await Clients.Caller.SendAsync("ErrorOccurred", "Không th? g?i tin nh?n: " + ex.Message);
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

                    // Lýu thông báo r?i ph?ng
                    var roomId = int.Parse(roomName.Replace("Room_", ""));
                    var systemMessage = new ChatMessage
                    {
                        Content = $"{userName} ð? r?i ph?ng chat",
                        RoomId = roomId,
                        SenderId = Context.UserIdentifier,
                        IsSystemMessage = true
                    };
                    await _chatRepository.AddMessageAsync(systemMessage);

                    // Thông báo cho t?t c? ngý?i dùng trong ph?ng
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