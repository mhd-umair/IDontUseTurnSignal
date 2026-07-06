namespace TurnSignalTracker.Models;

public class AutomationSettings
{
    public bool AutoSubmitAfterAiProcessing { get; set; }
    public bool EnableAiInsights { get; set; } = true;
    public int InsightsRefreshMinutes { get; set; } = 30;
    public string? ApiKey { get; set; }
}

public class AutomationResult
{
    public bool Success { get; set; }
    public bool AutoSaved { get; set; }
    public BadDriver? Driver { get; set; }
    public string? Message { get; set; }
}

public class AutomateViolationRequest
{
    public string Description { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool? AutoSave { get; set; }
}
