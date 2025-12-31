using System.Security.Cryptography;
using System.Text;
using TurnSignalViolationTracker.Core.DTOs;
using TurnSignalViolationTracker.Core.Interfaces;
using TurnSignalViolationTracker.Core.Models;

namespace TurnSignalViolationTracker.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<LoginResultDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
        
        if (user == null)
        {
            return new LoginResultDto
            {
                Success = false,
                ErrorMessage = "Invalid username or password"
            };
        }

        if (!user.IsActive)
        {
            return new LoginResultDto
            {
                Success = false,
                ErrorMessage = "Account is disabled"
            };
        }

        if (!VerifyPassword(loginDto.Password, user.Salt, user.PasswordHash))
        {
            return new LoginResultDto
            {
                Success = false,
                ErrorMessage = "Invalid username or password"
            };
        }

        await _userRepository.UpdateLastLoginAsync(user.Id);

        return new LoginResultDto
        {
            Success = true,
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName
        };
    }

    public async Task<bool> RegisterAsync(string username, string password, string fullName, string email)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(username);
        if (existingUser != null)
        {
            return false;
        }

        var salt = GenerateSalt();
        var hash = HashPassword(password, salt);

        var user = new User
        {
            Username = username,
            PasswordHash = hash,
            Salt = salt,
            FullName = fullName,
            Email = email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);
        return true;
    }

    public string GenerateSalt()
    {
        var saltBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    public string HashPassword(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        var combined = Encoding.UTF8.GetBytes(password + salt);
        var hashBytes = sha256.ComputeHash(combined);
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string salt, string hash)
    {
        var computedHash = HashPassword(password, salt);
        return computedHash == hash;
    }
}
