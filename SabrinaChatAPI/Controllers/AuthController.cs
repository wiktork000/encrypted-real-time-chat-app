using ChatApp.Data;
using ChatApp.DTOs;
using ChatApp.Services;
using ChatApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly SabrinaChatDbContext _context;
        private readonly IKeyService _keyService;

        public AuthController(IAuthService authService, IKeyService keyService, SabrinaChatDbContext context)
        {
            _authService = authService;
            _keyService = keyService;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto.Email, loginDto.Password);
            if (!result.Success)
            {
                return Unauthorized(new { Status = 400, Error = "Invalid credentials" });
            }

            return Ok(new { Status =  200, Error = "", Token = result.Token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto.Username, registerDto.Email, registerDto.Password);

            if (!result.Success || result.User == null)
            {
                return BadRequest(new { Status = 400, Error = result.ErrorMessage });
            }
            
            var keyResult = await _keyService.CreateUserKeyAsync(result.User.Id, registerDto.Password);
            if (!keyResult.Success)
            {
                return BadRequest(new { Status = 500, Error = "Internal server error" });
            }
            else
            {
                return Ok(new { Status = 200, Error = "", Token = result.Token });
            }
        }
    }
}
