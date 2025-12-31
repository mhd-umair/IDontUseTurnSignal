using TurnSignalViolationTracker.Core.Enums;

namespace TurnSignalViolationTracker.Core.DTOs;

public class ViolationExtractionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    
    public string? CarMake { get; set; }
    public string? CarModel { get; set; }
    public string? CarColor { get; set; }
    public int? CarYear { get; set; }
    public ViolationType? ViolationType { get; set; }
    public string? LocationDescription { get; set; }
    public string? OriginalTranscript { get; set; }
}
