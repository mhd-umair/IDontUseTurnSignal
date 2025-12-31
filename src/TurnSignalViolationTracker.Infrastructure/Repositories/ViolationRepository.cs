using Dapper;
using TurnSignalViolationTracker.Core.DTOs;
using TurnSignalViolationTracker.Core.Interfaces;
using TurnSignalViolationTracker.Core.Models;

namespace TurnSignalViolationTracker.Infrastructure.Repositories;

public class ViolationRepository : IViolationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICarBrandRepository _carBrandRepository;

    public ViolationRepository(IDbConnectionFactory connectionFactory, ICarBrandRepository carBrandRepository)
    {
        _connectionFactory = connectionFactory;
        _carBrandRepository = carBrandRepository;
    }

    public async Task<Violation?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT v.*, u.FullName as CreatedByUserName 
            FROM Violations v
            LEFT JOIN Users u ON v.CreatedByUserId = u.Id
            WHERE v.Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Violation>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Violation>> GetAllAsync(int? limit = null, int? offset = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT v.*, u.FullName as CreatedByUserName 
            FROM Violations v
            LEFT JOIN Users u ON v.CreatedByUserId = u.Id
            ORDER BY v.OccurredAt DESC";
        
        if (limit.HasValue)
        {
            sql += " OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
            return await connection.QueryAsync<Violation>(sql, new { Offset = offset ?? 0, Limit = limit.Value });
        }
        
        return await connection.QueryAsync<Violation>(sql);
    }

    public async Task<IEnumerable<Violation>> GetByUserIdAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT v.*, u.FullName as CreatedByUserName 
            FROM Violations v
            LEFT JOIN Users u ON v.CreatedByUserId = u.Id
            WHERE v.CreatedByUserId = @UserId
            ORDER BY v.OccurredAt DESC";
        return await connection.QueryAsync<Violation>(sql, new { UserId = userId });
    }

    public async Task<int> CreateAsync(Violation violation)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Violations (CarMake, CarModel, CarColor, CarYear, ViolationType, 
                Latitude, Longitude, LocationDescription, City, State, Country,
                OccurredAt, CreatedAt, CreatedByUserId, OriginalVoiceTranscript, Notes)
            OUTPUT INSERTED.Id
            VALUES (@CarMake, @CarModel, @CarColor, @CarYear, @ViolationType,
                @Latitude, @Longitude, @LocationDescription, @City, @State, @Country,
                @OccurredAt, @CreatedAt, @CreatedByUserId, @OriginalVoiceTranscript, @Notes)";
        return await connection.ExecuteScalarAsync<int>(sql, violation);
    }

    public async Task<bool> UpdateAsync(Violation violation)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Violations 
            SET CarMake = @CarMake, CarModel = @CarModel, CarColor = @CarColor, CarYear = @CarYear,
                ViolationType = @ViolationType, Latitude = @Latitude, Longitude = @Longitude,
                LocationDescription = @LocationDescription, City = @City, State = @State, Country = @Country,
                OccurredAt = @OccurredAt, Notes = @Notes
            WHERE Id = @Id";
        var result = await connection.ExecuteAsync(sql, violation);
        return result > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Violations WHERE Id = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }

    public async Task<int> GetTotalCountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM Violations";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<DashboardStatistics> GetDashboardStatisticsAsync()
    {
        var stats = new DashboardStatistics();
        
        using var connection = _connectionFactory.CreateConnection();
        
        // Get total counts
        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddDays(-30);
        
        const string countsSql = @"
            SELECT 
                COUNT(*) as TotalViolations,
                SUM(CASE WHEN CAST(OccurredAt AS DATE) = CAST(@Today AS DATE) THEN 1 ELSE 0 END) as ViolationsToday,
                SUM(CASE WHEN OccurredAt >= @WeekAgo THEN 1 ELSE 0 END) as ViolationsThisWeek,
                SUM(CASE WHEN OccurredAt >= @MonthAgo THEN 1 ELSE 0 END) as ViolationsThisMonth
            FROM Violations";
        
        var counts = await connection.QueryFirstAsync<dynamic>(countsSql, new { Today = today, WeekAgo = weekAgo, MonthAgo = monthAgo });
        stats.TotalViolations = (int)counts.TotalViolations;
        stats.ViolationsToday = (int)counts.ViolationsToday;
        stats.ViolationsThisWeek = (int)counts.ViolationsThisWeek;
        stats.ViolationsThisMonth = (int)counts.ViolationsThisMonth;
        
        stats.ViolationsByBrand = (await GetViolationsByBrandAsync()).ToList();
        stats.ViolationsByColor = (await GetViolationsByColorAsync()).ToList();
        stats.ViolationsByType = (await GetViolationsByTypeAsync()).ToList();
        stats.ViolationsByHour = (await GetViolationsByHourAsync()).ToList();
        stats.ViolationsByDayOfWeek = (await GetViolationsByDayOfWeekAsync()).ToList();
        stats.ViolationsTrend = (await GetViolationsTrendAsync()).ToList();
        stats.ViolationLocations = (await GetViolationLocationsAsync()).ToList();
        
        return stats;
    }

    public async Task<IEnumerable<ViolationsByBrandDto>> GetViolationsByBrandAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                CarMake as Brand,
                COUNT(*) as Count
            FROM Violations
            GROUP BY CarMake
            ORDER BY Count DESC";
        
        var results = (await connection.QueryAsync<ViolationsByBrandDto>(sql)).ToList();
        var total = results.Sum(r => r.Count);
        
        // Get logo URLs from car brands
        var carBrands = await _carBrandRepository.GetAllAsync();
        var brandDict = carBrands.ToDictionary(b => b.Name.ToLower(), b => b.LogoUrl);
        
        foreach (var result in results)
        {
            result.Percentage = total > 0 ? Math.Round((double)result.Count / total * 100, 1) : 0;
            if (brandDict.TryGetValue(result.Brand.ToLower(), out var logoUrl))
            {
                result.LogoUrl = logoUrl;
            }
        }
        
        return results;
    }

    public async Task<IEnumerable<ViolationsByColorDto>> GetViolationsByColorAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                CarColor as Color,
                COUNT(*) as Count
            FROM Violations
            GROUP BY CarColor
            ORDER BY Count DESC";
        
        var results = (await connection.QueryAsync<ViolationsByColorDto>(sql)).ToList();
        var total = results.Sum(r => r.Count);
        
        foreach (var result in results)
        {
            result.Percentage = total > 0 ? Math.Round((double)result.Count / total * 100, 1) : 0;
        }
        
        return results;
    }

    public async Task<IEnumerable<ViolationsByTypeDto>> GetViolationsByTypeAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                ViolationType as ViolationTypeId,
                COUNT(*) as Count
            FROM Violations
            GROUP BY ViolationType
            ORDER BY Count DESC";
        
        var results = (await connection.QueryAsync<ViolationsByTypeDto>(sql)).ToList();
        var total = results.Sum(r => r.Count);
        
        var typeNames = new Dictionary<int, string>
        {
            { 1, "Left Turn Without Signal" },
            { 2, "Right Turn Without Signal" },
            { 3, "Lane Change Left Without Signal" },
            { 4, "Lane Change Right Without Signal" },
            { 5, "U-Turn Without Signal" }
        };
        
        foreach (var result in results)
        {
            result.ViolationType = typeNames.GetValueOrDefault(result.ViolationTypeId, "Unknown");
            result.Percentage = total > 0 ? Math.Round((double)result.Count / total * 100, 1) : 0;
        }
        
        return results;
    }

    public async Task<IEnumerable<ViolationsByTimeDto>> GetViolationsByHourAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                DATEPART(HOUR, OccurredAt) as Hour,
                COUNT(*) as Count
            FROM Violations
            GROUP BY DATEPART(HOUR, OccurredAt)
            ORDER BY Hour";
        
        return await connection.QueryAsync<ViolationsByTimeDto>(sql);
    }

    public async Task<IEnumerable<ViolationsByDayDto>> GetViolationsByDayOfWeekAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                DATEPART(WEEKDAY, OccurredAt) as DayNumber,
                COUNT(*) as Count
            FROM Violations
            GROUP BY DATEPART(WEEKDAY, OccurredAt)
            ORDER BY DayNumber";
        
        var results = (await connection.QueryAsync<ViolationsByDayDto>(sql)).ToList();
        
        var dayNames = new Dictionary<int, string>
        {
            { 1, "Sunday" }, { 2, "Monday" }, { 3, "Tuesday" }, { 4, "Wednesday" },
            { 5, "Thursday" }, { 6, "Friday" }, { 7, "Saturday" }
        };
        
        foreach (var result in results)
        {
            result.DayOfWeek = dayNames.GetValueOrDefault(result.DayNumber, "Unknown");
        }
        
        return results;
    }

    public async Task<IEnumerable<ViolationsTrendDto>> GetViolationsTrendAsync(int days = 30)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                CAST(OccurredAt AS DATE) as Date,
                COUNT(*) as Count
            FROM Violations
            WHERE OccurredAt >= @StartDate
            GROUP BY CAST(OccurredAt AS DATE)
            ORDER BY Date";
        
        var startDate = DateTime.UtcNow.Date.AddDays(-days);
        return await connection.QueryAsync<ViolationsTrendDto>(sql, new { StartDate = startDate });
    }

    public async Task<IEnumerable<ViolationLocationDto>> GetViolationLocationsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                Latitude,
                Longitude,
                LocationDescription,
                COUNT(*) as Count
            FROM Violations
            WHERE Latitude IS NOT NULL AND Longitude IS NOT NULL
            GROUP BY Latitude, Longitude, LocationDescription
            ORDER BY Count DESC";
        
        return await connection.QueryAsync<ViolationLocationDto>(sql);
    }
}
