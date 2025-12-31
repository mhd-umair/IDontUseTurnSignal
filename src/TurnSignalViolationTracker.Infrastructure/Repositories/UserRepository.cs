using Dapper;
using TurnSignalViolationTracker.Core.Interfaces;
using TurnSignalViolationTracker.Core.Models;

namespace TurnSignalViolationTracker.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Users WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Users WHERE Username = @Username";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Users ORDER BY Username";
        return await connection.QueryAsync<User>(sql);
    }

    public async Task<int> CreateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Users (Username, PasswordHash, Salt, FullName, Email, IsActive, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Username, @PasswordHash, @Salt, @FullName, @Email, @IsActive, @CreatedAt)";
        return await connection.ExecuteScalarAsync<int>(sql, user);
    }

    public async Task<bool> UpdateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Users 
            SET Username = @Username, 
                PasswordHash = @PasswordHash, 
                Salt = @Salt, 
                FullName = @FullName, 
                Email = @Email, 
                IsActive = @IsActive
            WHERE Id = @Id";
        var result = await connection.ExecuteAsync(sql, user);
        return result > 0;
    }

    public async Task<bool> UpdateLastLoginAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE Users SET LastLoginAt = @LastLoginAt WHERE Id = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = userId, LastLoginAt = DateTime.UtcNow });
        return result > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Users WHERE Id = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }
}
