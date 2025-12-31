using TurnSignalTracker.Models;

namespace TurnSignalTracker.Services;

public interface IDriverService
{
    Task<IEnumerable<BadDriver>> GetAllAsync();
    Task<IEnumerable<BadDriverStats>> GetStatsAsync();
    Task AddAsync(BadDriver driver);
}

public class BadDriverStats 
{
    public string CarMake { get; set; } = string.Empty;
    public int Count { get; set; }
}
