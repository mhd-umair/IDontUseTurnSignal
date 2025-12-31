using TurnSignalViolationTracker.Core.Enums;

namespace TurnSignalViolationTracker.Core.DTOs;

public class ViolationCreateDto
{
    public string CarMake { get; set; } = string.Empty;
    public string CarModel { get; set; } = string.Empty;
    public string CarColor { get; set; } = string.Empty;
    public int? CarYear { get; set; }
    public ViolationType ViolationType { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string LocationDescription { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string? OriginalVoiceTranscript { get; set; }
    public string? Notes { get; set; }
}
