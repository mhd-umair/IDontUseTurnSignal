using TurnSignalViolationTracker.Core.DTOs;
using TurnSignalViolationTracker.Core.Models;

namespace TurnSignalViolationTracker.Core.Interfaces;

public interface IViolationRepository
{
    Task<Violation?> GetByIdAsync(int id);
    Task<IEnumerable<Violation>> GetAllAsync(int? limit = null, int? offset = null);
    Task<IEnumerable<Violation>> GetByUserIdAsync(int userId);
    Task<int> CreateAsync(Violation violation);
    Task<bool> UpdateAsync(Violation violation);
    Task<bool> DeleteAsync(int id);
    Task<int> GetTotalCountAsync();
    
    // Statistics queries
    Task<DashboardStatistics> GetDashboardStatisticsAsync();
    Task<IEnumerable<ViolationsByBrandDto>> GetViolationsByBrandAsync();
    Task<IEnumerable<ViolationsByColorDto>> GetViolationsByColorAsync();
    Task<IEnumerable<ViolationsByTypeDto>> GetViolationsByTypeAsync();
    Task<IEnumerable<ViolationsByTimeDto>> GetViolationsByHourAsync();
    Task<IEnumerable<ViolationsByDayDto>> GetViolationsByDayOfWeekAsync();
    Task<IEnumerable<ViolationsTrendDto>> GetViolationsTrendAsync(int days = 30);
    Task<IEnumerable<ViolationLocationDto>> GetViolationLocationsAsync();
}
