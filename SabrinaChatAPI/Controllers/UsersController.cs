using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ChatApp.Data;
using ChatApp.DTOs;
using ChatApp.Services;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController(SabrinaChatDbContext context, IAuthService authService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                CreatedAt = user.CreatedAt
            };

            return Ok(userDto);
        }

        [HttpGet("me")]
        public async Task<ActionResult<CurrentUserDto>> GetCurrentUser()
        {
            var userId = authService.GetCurrentUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await context.Users.FindAsync(userId);

            if (user == null)
            {
                return BadRequest();
            }

            var userDto = new CurrentUserDto
            {
                Id = user.Id,
                Name = user.Name,
                PublicKey = user.PublicKey,
                PrivateKey = user.PrivateKey,
                CreatedAt = user.CreatedAt
            };

            return Ok(userDto);
        }
    }
}
