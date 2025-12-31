using TurnSignalViolationTracker.Core.Models;

namespace TurnSignalViolationTracker.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> GetAllAsync();
    Task<int> CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> UpdateLastLoginAsync(int userId);
    Task<bool> DeleteAsync(int id);
}
