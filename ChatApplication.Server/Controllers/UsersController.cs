using ChatApplication.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.API.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var users = await _userService.GetAllUsersAsync();

            var userList = users
                .Where(u => u.Id != currentUserId)
                .Select(u => new
                {
                    id = u.Id,
                    username = u.Username,
                    isOnline = u.IsOnline,
                    lastSeen = u.LastSeen
                });

            return Ok(userList);
        }
        [Authorize]
        [HttpGet("chats")]
        public async Task<IActionResult> GetChatUsers()
        {
            var currentUserId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var users = await _userService.GetChatUsersAsync(currentUserId);

            return Ok(users.Select(u => new
            {
                id = u.Id,
                username = u.Username,
                isOnline = u.IsOnline,
                lastSeen = u.LastSeen
            }));
        }
    }
}