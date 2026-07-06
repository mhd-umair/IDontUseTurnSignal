using TurnSignalTracker.Models;

namespace TurnSignalTracker.Services;

public interface IAutomationService
{
    Task<AutomationResult> ProcessViolationAsync(string description, double? lat, double? lon, bool? autoSaveOverride = null);
}
