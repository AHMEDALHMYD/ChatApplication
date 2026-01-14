using ChatApplication.Server.Models.Profiles;
using ChatApplication.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatApplication.Server.Hubs
{

    [Authorize]
    public class ChatHub : Hub
    {
        private readonly UserService _userService;
        private readonly MessageService _messageService;

        public ChatHub(UserService userService, MessageService messageService)
        {
            _userService = userService;
            _messageService = messageService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                await _userService.UpdateUserStatusAsync(userId.Value, true);
                await Clients.All.SendAsync("UserStatusChanged", userId.Value, true);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                await _userService.UpdateUserStatusAsync(userId.Value, false);
                await Clients.All.SendAsync("UserStatusChanged", userId.Value, false);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(int receiverId, string message)
        {
            var senderId = GetUserId();
            if (!senderId.HasValue) return;

            var savedMessage = await _messageService.SaveMessageAsync(
                senderId.Value,
                receiverId,
                message
            );

            var messageDto = new MessageDto
            {
                Id = savedMessage.Id,
                SenderId = savedMessage.SenderId,
                ReceiverId = savedMessage.ReceiverId,
                Content = message,
                Timestamp = savedMessage.Timestamp,
            };
            await Clients.Users(
                senderId.Value.ToString(),
                receiverId.ToString()
            ).SendAsync("ReceiveMessage", messageDto);
        }
        private int? GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : null;
        }
    }
}