using TurnSignalViolationTracker.Core.Enums;

namespace TurnSignalViolationTracker.Infrastructure.Configuration;

public class AISettings
{
    public AIProvider Provider { get; set; } = AIProvider.OpenAI;
    public OpenAISettings OpenAI { get; set; } = new();
    public GeminiSettings Gemini { get; set; } = new();
}

public class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o";
    public string WhisperModel { get; set; } = "whisper-1";
}

public class GeminiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.0-flash-exp";
}
