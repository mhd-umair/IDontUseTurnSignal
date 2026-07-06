using Microsoft.Extensions.Options;
using TurnSignalTracker.Models;

namespace TurnSignalTracker.Services;

public class AutomationService : IAutomationService
{
    private readonly IAIService _aiService;
    private readonly IDriverService _driverService;
    private readonly AutomationSettings _settings;

    public AutomationService(
        IAIService aiService,
        IDriverService driverService,
        IOptions<AutomationSettings> settings)
    {
        _aiService = aiService;
        _driverService = driverService;
        _settings = settings.Value;
    }

    public async Task<AutomationResult> ProcessViolationAsync(
        string description,
        double? lat,
        double? lon,
        bool? autoSaveOverride = null)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return new AutomationResult
            {
                Success = false,
                Message = "Description is required."
            };
        }

        var driver = await _aiService.ProcessVoiceLogAsync(description.Trim(), lat, lon);
        var shouldAutoSave = autoSaveOverride ?? _settings.AutoSubmitAfterAiProcessing;

        if (!shouldAutoSave)
        {
            return new AutomationResult
            {
                Success = true,
                AutoSaved = false,
                Driver = driver,
                Message = "AI extraction complete. Review before saving."
            };
        }

        try
        {
            await _driverService.AddAsync(driver);
            return new AutomationResult
            {
                Success = true,
                AutoSaved = true,
                Driver = driver,
                Message = "Violation logged automatically."
            };
        }
        catch (Exception ex)
        {
            return new AutomationResult
            {
                Success = true,
                AutoSaved = false,
                Driver = driver,
                Message = $"AI extraction succeeded but auto-save failed: {ex.Message}"
            };
        }
    }
}
