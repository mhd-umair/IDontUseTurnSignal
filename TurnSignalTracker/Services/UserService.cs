using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using TurnSignalTracker.Models;

namespace TurnSignalTracker.Services;

public interface IUserService
{
    Task<User?> LoginAsync(string username, string password);
}

public class UserService : IUserService
{
    private readonly string _connectionString;

    public UserService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<User?> LoginAsync(string username, string password)
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM Users WHERE Username = @Username";
        var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });

        if (user == null) return null;

        var inputHash = ComputeSha256Hash(password);
        if (user.PasswordHash == inputHash)
        {
            return user;
        }

        return null;
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
