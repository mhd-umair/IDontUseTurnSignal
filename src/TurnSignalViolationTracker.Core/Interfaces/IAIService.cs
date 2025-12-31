using TurnSignalViolationTracker.Core.DTOs;

namespace TurnSignalViolationTracker.Core.Interfaces;

public interface IAIService
{
    Task<ViolationExtractionResult> ExtractViolationFromAudioAsync(byte[] audioData, string mimeType);
    Task<ViolationExtractionResult> ExtractViolationFromTextAsync(string text);
}
