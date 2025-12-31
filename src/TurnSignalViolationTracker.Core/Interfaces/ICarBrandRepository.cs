using TurnSignalViolationTracker.Core.Models;

namespace TurnSignalViolationTracker.Core.Interfaces;

public interface ICarBrandRepository
{
    Task<CarBrand?> GetByIdAsync(int id);
    Task<CarBrand?> GetByNameAsync(string name);
    Task<IEnumerable<CarBrand>> GetAllAsync();
    Task<int> CreateAsync(CarBrand carBrand);
    Task<bool> UpdateAsync(CarBrand carBrand);
    Task<bool> DeleteAsync(int id);
}
