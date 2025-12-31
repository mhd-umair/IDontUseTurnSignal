using Dapper;
using TurnSignalViolationTracker.Core.Interfaces;
using TurnSignalViolationTracker.Core.Models;

namespace TurnSignalViolationTracker.Infrastructure.Repositories;

public class CarBrandRepository : ICarBrandRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CarBrandRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CarBrand?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM CarBrands WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<CarBrand>(sql, new { Id = id });
    }

    public async Task<CarBrand?> GetByNameAsync(string name)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM CarBrands WHERE LOWER(Name) = LOWER(@Name)";
        return await connection.QueryFirstOrDefaultAsync<CarBrand>(sql, new { Name = name });
    }

    public async Task<IEnumerable<CarBrand>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM CarBrands WHERE IsActive = 1 ORDER BY Name";
        return await connection.QueryAsync<CarBrand>(sql);
    }

    public async Task<int> CreateAsync(CarBrand carBrand)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO CarBrands (Name, LogoUrl, IsActive)
            OUTPUT INSERTED.Id
            VALUES (@Name, @LogoUrl, @IsActive)";
        return await connection.ExecuteScalarAsync<int>(sql, carBrand);
    }

    public async Task<bool> UpdateAsync(CarBrand carBrand)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE CarBrands 
            SET Name = @Name, LogoUrl = @LogoUrl, IsActive = @IsActive
            WHERE Id = @Id";
        var result = await connection.ExecuteAsync(sql, carBrand);
        return result > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM CarBrands WHERE Id = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }
}
