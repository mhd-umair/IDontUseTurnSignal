namespace TurnSignalViolationTracker.Core.Models;

public class CarBrand
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
