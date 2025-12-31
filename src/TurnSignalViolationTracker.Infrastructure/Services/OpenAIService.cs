using System.ClientModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
using TurnSignalViolationTracker.Core.DTOs;
using TurnSignalViolationTracker.Core.Enums;
using TurnSignalViolationTracker.Core.Interfaces;
using TurnSignalViolationTracker.Infrastructure.Configuration;

namespace TurnSignalViolationTracker.Infrastructure.Services;

public class OpenAIService : IAIService
{
    private readonly OpenAIClient _client;
    private readonly AISettings _settings;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(IOptions<AISettings> settings, ILogger<OpenAIService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _client = new OpenAIClient(_settings.OpenAI.ApiKey);
    }

    public async Task<ViolationExtractionResult> ExtractViolationFromAudioAsync(byte[] audioData, string mimeType)
    {
        try
        {
            // First, transcribe the audio using Whisper
            var audioClient = _client.GetAudioClient(_settings.OpenAI.WhisperModel);
            
            var extension = mimeType switch
            {
                "audio/webm" => "webm",
                "audio/mp3" => "mp3",
                "audio/mpeg" => "mp3",
                "audio/wav" => "wav",
                "audio/ogg" => "ogg",
                _ => "webm"
            };

            using var audioStream = new MemoryStream(audioData);
            var transcription = await audioClient.TranscribeAudioAsync(audioStream, $"audio.{extension}");
            
            var transcript = transcription.Value.Text;
            _logger.LogInformation("Transcribed audio: {Transcript}", transcript);

            // Now extract structured data from the transcript
            return await ExtractViolationFromTextAsync(transcript);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting violation from audio");
            return new ViolationExtractionResult
            {
                Success = false,
                ErrorMessage = $"Failed to process audio: {ex.Message}"
            };
        }
    }

    public async Task<ViolationExtractionResult> ExtractViolationFromTextAsync(string text)
    {
        try
        {
            var chatClient = _client.GetChatClient(_settings.OpenAI.Model);
            
            var systemPrompt = GetExtractionPrompt();
            
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(text)
            };

            var response = await chatClient.CompleteChatAsync(messages);
            var responseText = response.Value.Content[0].Text;
            
            _logger.LogInformation("AI Response: {Response}", responseText);

            return ParseExtractionResponse(responseText, text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting violation from text");
            return new ViolationExtractionResult
            {
                Success = false,
                ErrorMessage = $"Failed to extract violation data: {ex.Message}",
                OriginalTranscript = text
            };
        }
    }

    private static string GetExtractionPrompt()
    {
        return @"You are an AI assistant that extracts traffic violation information from voice descriptions. 
Extract the following information from the user's description and return it as JSON:

- carMake: The manufacturer/brand of the car (e.g., Toyota, Honda, Ford, BMW)
- carModel: The specific model (e.g., Camry, Civic, F-150, 3 Series)
- carColor: The color of the car
- carYear: The year of the car (if mentioned, otherwise null)
- violationType: One of the following values:
  * 1 = Left Turn Without Signal
  * 2 = Right Turn Without Signal  
  * 3 = Lane Change Left Without Signal
  * 4 = Lane Change Right Without Signal
  * 5 = U-Turn Without Signal
- locationDescription: Any location details mentioned

If information is not provided, use null for that field.
Return ONLY valid JSON, no other text. Example:
{
  ""carMake"": ""Toyota"",
  ""carModel"": ""Camry"",
  ""carColor"": ""Black"",
  ""carYear"": 2020,
  ""violationType"": 3,
  ""locationDescription"": ""Main Street and 5th Avenue""
}";
    }

    private ViolationExtractionResult ParseExtractionResponse(string response, string originalText)
    {
        try
        {
            // Clean up the response - remove markdown code blocks if present
            var jsonText = response.Trim();
            if (jsonText.StartsWith("```json"))
            {
                jsonText = jsonText.Substring(7);
            }
            else if (jsonText.StartsWith("```"))
            {
                jsonText = jsonText.Substring(3);
            }
            if (jsonText.EndsWith("```"))
            {
                jsonText = jsonText.Substring(0, jsonText.Length - 3);
            }
            jsonText = jsonText.Trim();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var extracted = JsonSerializer.Deserialize<ExtractedData>(jsonText, options);

            if (extracted == null)
            {
                return new ViolationExtractionResult
                {
                    Success = false,
                    ErrorMessage = "Failed to parse AI response",
                    OriginalTranscript = originalText
                };
            }

            return new ViolationExtractionResult
            {
                Success = true,
                CarMake = extracted.CarMake,
                CarModel = extracted.CarModel,
                CarColor = extracted.CarColor,
                CarYear = extracted.CarYear,
                ViolationType = extracted.ViolationType.HasValue ? (ViolationType)extracted.ViolationType.Value : null,
                LocationDescription = extracted.LocationDescription,
                OriginalTranscript = originalText
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI response as JSON: {Response}", response);
            return new ViolationExtractionResult
            {
                Success = false,
                ErrorMessage = "Failed to parse AI response",
                OriginalTranscript = originalText
            };
        }
    }

    private class ExtractedData
    {
        public string? CarMake { get; set; }
        public string? CarModel { get; set; }
        public string? CarColor { get; set; }
        public int? CarYear { get; set; }
        public int? ViolationType { get; set; }
        public string? LocationDescription { get; set; }
    }
}
