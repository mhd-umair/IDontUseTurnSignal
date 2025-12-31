using TurnSignalViolationTracker.Core.DTOs;
using TurnSignalViolationTracker.Core.Models;

namespace TurnSignalViolationTracker.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(LoginDto loginDto);
    Task<bool> RegisterAsync(string username, string password, string fullName, string email);
    string HashPassword(string password, string salt);
    string GenerateSalt();
    bool VerifyPassword(string password, string salt, string hash);
}
