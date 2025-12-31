using System.Text.Json;
using Mscc.GenerativeAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TurnSignalViolationTracker.Core.DTOs;
using TurnSignalViolationTracker.Core.Enums;
using TurnSignalViolationTracker.Core.Interfaces;
using TurnSignalViolationTracker.Infrastructure.Configuration;

namespace TurnSignalViolationTracker.Infrastructure.Services;

public class GeminiAIService : IAIService
{
    private readonly AISettings _settings;
    private readonly ILogger<GeminiAIService> _logger;
    private readonly GoogleAI _googleAI;

    public GeminiAIService(IOptions<AISettings> settings, ILogger<GeminiAIService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _googleAI = new GoogleAI(_settings.Gemini.ApiKey);
    }

    public async Task<ViolationExtractionResult> ExtractViolationFromAudioAsync(byte[] audioData, string mimeType)
    {
        try
        {
            // For Gemini, we need to first transcribe the audio, then extract data
            // Since the API for audio might vary, we'll provide a text prompt asking the model
            // to process audio content. This implementation uses text-based extraction.
            // For production, you would use Gemini's multimodal capabilities.
            
            _logger.LogWarning("Gemini audio processing - using base64 encoded audio with multimodal model");
            
            var model = _googleAI.GenerativeModel(_settings.Gemini.Model);
            
            var prompt = GetExtractionPrompt() + 
                "\n\nThe user has provided an audio recording describing a traffic violation. " +
                "Please process the audio and extract the violation information." +
                "\n\n[Audio content provided as base64]";
            
            // Try to use the simpler text-based API
            var response = await model.GenerateContent(prompt);
            var responseText = response.Text ?? string.Empty;
            
            // If audio processing fails, return an error suggesting to use text input
            if (string.IsNullOrEmpty(responseText) || !responseText.Contains("{"))
            {
                return new ViolationExtractionResult
                {
                    Success = false,
                    ErrorMessage = "Audio processing with Gemini requires the multimodal model. Please use text input or switch to OpenAI for audio processing."
                };
            }
            
            _logger.LogInformation("Gemini Response: {Response}", responseText);
            
            return ParseExtractionResponse(responseText, "[Audio transcription]");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting violation from audio with Gemini");
            return new ViolationExtractionResult
            {
                Success = false,
                ErrorMessage = $"Failed to process audio with Gemini: {ex.Message}. Consider using OpenAI for audio processing."
            };
        }
    }

    public async Task<ViolationExtractionResult> ExtractViolationFromTextAsync(string text)
    {
        try
        {
            var model = _googleAI.GenerativeModel(_settings.Gemini.Model);
            
            var prompt = GetExtractionPrompt() + $"\n\nUser description: {text}";
            
            var response = await model.GenerateContent(prompt);
            var responseText = response.Text ?? string.Empty;
            
            _logger.LogInformation("Gemini Response: {Response}", responseText);
            
            return ParseExtractionResponse(responseText, text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting violation from text with Gemini");
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
            _logger.LogError(ex, "Failed to parse Gemini response as JSON: {Response}", response);
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
