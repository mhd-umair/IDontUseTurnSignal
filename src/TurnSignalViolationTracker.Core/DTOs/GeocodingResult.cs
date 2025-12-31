namespace TurnSignalViolationTracker.Core.DTOs;

public class GeocodingResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? FormattedAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Street { get; set; }
}
