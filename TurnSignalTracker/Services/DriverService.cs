using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using TurnSignalTracker.Models;

namespace TurnSignalTracker.Services;

public class DriverService : IDriverService
{
    private readonly string _connectionString;

    public DriverService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<IEnumerable<BadDriver>> GetAllAsync()
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM BadDrivers ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<BadDriver>(sql);
    }

    public async Task<IEnumerable<BadDriverStats>> GetStatsAsync()
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT CarMake, COUNT(*) as Count 
            FROM BadDrivers 
            GROUP BY CarMake 
            ORDER BY Count DESC";
        return await connection.QueryAsync<BadDriverStats>(sql);
    }

    public async Task AddAsync(BadDriver driver)
    {
        using var connection = CreateConnection();
        const string sql = @"
            INSERT INTO BadDrivers (CarMake, CarModel, CarColor, CarYear, Location, Latitude, Longitude, ViolationType, CreatedAt)
            VALUES (@CarMake, @CarModel, @CarColor, @CarYear, @Location, @Latitude, @Longitude, @ViolationType, @CreatedAt)";
        await connection.ExecuteAsync(sql, driver);
    }
}
