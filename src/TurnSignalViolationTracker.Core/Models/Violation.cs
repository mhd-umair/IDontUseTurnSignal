using TurnSignalViolationTracker.Core.Enums;

namespace TurnSignalViolationTracker.Core.Models;

public class Violation
{
    public int Id { get; set; }
    
    // Car details
    public string CarMake { get; set; } = string.Empty;
    public string CarModel { get; set; } = string.Empty;
    public string CarColor { get; set; } = string.Empty;
    public int? CarYear { get; set; }
    
    // Violation details
    public ViolationType ViolationType { get; set; }
    
    // Location details
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string LocationDescription { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    
    // Metadata
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
    public string? OriginalVoiceTranscript { get; set; }
    public string? Notes { get; set; }
    
    // Navigation property (not stored in DB, used for display)
    public string? CreatedByUserName { get; set; }
}
