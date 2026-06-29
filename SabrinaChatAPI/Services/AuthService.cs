using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ChatApp.Data;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RegisterAsync(string username, string email, string password);
    int? GetUserIdFromToken(string token);
    int? GetCurrentUserId(ClaimsPrincipal user);
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? ErrorMessage { get; set; }
    public User? User { get; set; }
}

public class AuthService : IAuthService
{
    private readonly SabrinaChatDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(SabrinaChatDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            var userCredentials = await _context.UsersCredentials
                .Include(uc => uc.User)
                .FirstOrDefaultAsync(uc => uc.Email.ToLower() == email.ToLower());

            if (userCredentials == null)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Invalid email or password"
                };
            }

            if (!VerifyPassword(password, userCredentials.PasswordHash))
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Invalid email or password"
                };
            }

            var token = GenerateJwtToken(userCredentials.User);

            return new AuthResult
            {
                Success = true,
                Token = token,
                User = userCredentials.User
            };
        }
        catch
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "An error occurred during login"
            };
        }
    }

    public async Task<AuthResult> RegisterAsync(string username, string email, string password)
    {
        try
        {
            // Check if email already exists
            var existingCredentials = await _context.UsersCredentials
                .FirstOrDefaultAsync(uc => uc.Email.ToLower() == email.ToLower());

            if (existingCredentials != null)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Email already exists"
                };
            }

            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Name.ToLower() == username.ToLower());

            if (existingUser != null)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Username already exists"
                };
            }

            // Create new user
            var user = new User
            {
                Name = username,
                PublicKey = "", // Set to empty string as requested
                PrivateKey = "" // Set to empty string as requested
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create user credentials
            var userCredentials = new UserCredentials
            {
                UserId = user.Id,
                Email = email,
                PasswordHash = HashPassword(password)
            };

            _context.UsersCredentials.Add(userCredentials);
            await _context.SaveChangesAsync();

            // Generate token
            var token = GenerateJwtToken(user);

            return new AuthResult
            {
                Success = true,
                Token = token,
                User = user
            };
        }
        catch
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "An error occurred during registration"
            };
        }
    }

    public int? GetUserIdFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
        }
        catch
        {
            // Token is invalid
        }

        return null;
    }

    public int? GetCurrentUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, GetUserEmail(user.Id))
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? "your-secret-key-here-make-it-long-enough"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "chatapp",
            audience: _configuration["Jwt:Audience"] ?? "chatapp",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "salt-passwd"));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        // Using BCrypt for password verification
        // return BCrypt.Net.BCrypt.Verify(password, hashedPassword);

        // Simple SHA256 verification (matching the hashing method above)
        var hashOfInput = HashPassword(password);
        return hashOfInput == hashedPassword;
    }

    private string GetUserEmail(int userId)
    {
        var credentials = _context.UsersCredentials.FirstOrDefault(uc => uc.UserId == userId);
        return credentials?.Email ?? "";
    }
}