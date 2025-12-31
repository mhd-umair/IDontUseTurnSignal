using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using TurnSignalTracker.Models;

namespace TurnSignalTracker.Services;

public interface IAIService
{
    Task<BadDriver> ProcessVoiceLogAsync(string transcript, double? lat, double? lon);
}

public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AIService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<BadDriver> ProcessVoiceLogAsync(string transcript, double? lat, double? lon)
    {
        var provider = _configuration["AISettings:Provider"];
        
        BadDriver driver = new BadDriver
        {
             Latitude = lat,
             Longitude = lon,
             CreatedAt = DateTime.Now
        };

        try 
        {
            if (string.Equals(provider, "OpenAI", StringComparison.OrdinalIgnoreCase))
            {
                driver = await ProcessWithOpenAI(transcript, driver);
            }
            else if (string.Equals(provider, "Gemini", StringComparison.OrdinalIgnoreCase))
            {
                driver = await ProcessWithGemini(transcript, driver);
            }
            else 
            {
                driver = ProcessWithMock(transcript, driver);
            }
        }
        catch (Exception ex)
        {
             Console.WriteLine($"AI Processing failed: {ex.Message}. Falling back to mock.");
             driver = ProcessWithMock(transcript, driver);
        }

        // Final cleanup
        if (string.IsNullOrEmpty(driver.Location))
        {
            if (lat.HasValue && lon.HasValue) 
                driver.Location = $"{lat:F4}, {lon:F4}";
            else
                driver.Location = "Unknown Location";
        }

        return driver;
    }

    private async Task<BadDriver> ProcessWithOpenAI(string transcript, BadDriver driver)
    {
        var apiKey = _configuration["AISettings:OpenAI:ApiKey"];
        var model = _configuration["AISettings:OpenAI:Model"] ?? "gpt-3.5-turbo";

        if (string.IsNullOrEmpty(apiKey)) throw new InvalidOperationException("OpenAI API Key missing");

        var systemPrompt = @"You are a data extraction assistant for a traffic violation tracker app. 
        Extract the following fields from the user's voice transcript describing a bad driver: 
        CarMake, CarModel, CarColor, CarYear (int, null if not found), ViolationType (short description), Location (street name or address).
        Return ONLY valid JSON.";

        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = transcript }
            },
            temperature = 0.1
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {apiKey}");
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return ParseAIResponse(content, driver);
    }

    private async Task<BadDriver> ProcessWithGemini(string transcript, BadDriver driver)
    {
        var apiKey = _configuration["AISettings:Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey)) throw new InvalidOperationException("Gemini API Key missing");

        var prompt = $@"Extract traffic violation data from this text: ""{transcript}"".
        Return a JSON object with keys: CarMake, CarModel, CarColor, CarYear (number or null), ViolationType, Location. 
        Do not use markdown blocks. Return only raw JSON.";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}";
        
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        
        // Gemini response structure is deeply nested
        // candidates[0].content.parts[0].text
        var content = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return ParseAIResponse(content, driver);
    }

    private BadDriver ParseAIResponse(string? jsonContent, BadDriver existingDriver)
    {
        if (string.IsNullOrEmpty(jsonContent)) return existingDriver;

        try 
        {
            // Clean up markdown code blocks if present (common with LLMs)
            jsonContent = jsonContent.Replace("```json", "").Replace("```", "").Trim();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var extracted = JsonSerializer.Deserialize<AIResponseModel>(jsonContent, options);

            if (extracted != null)
            {
                existingDriver.CarMake = extracted.CarMake ?? "Unknown";
                existingDriver.CarModel = extracted.CarModel ?? "Unknown";
                existingDriver.CarColor = extracted.CarColor ?? "Unknown";
                existingDriver.CarYear = extracted.CarYear;
                existingDriver.ViolationType = extracted.ViolationType ?? "Unknown Violation";
                if (!string.IsNullOrEmpty(extracted.Location))
                {
                    existingDriver.Location = extracted.Location;
                }
            }
        } 
        catch (Exception ex)
        {
            Console.WriteLine($"JSON Parsing error: {ex.Message}");
            // Fallback to manual/mock if JSON fails
        }

        return existingDriver;
    }

    private BadDriver ProcessWithMock(string transcript, BadDriver driver)
    {
         // Rudimentary "AI" using Regex for demo purposes
        // Expected format examples: "A black BMW X5 2022 turned left at Main Street"
        
        // Extract Year
        var yearMatch = Regex.Match(transcript, @"\b(19|20)\d{2}\b");
        if (yearMatch.Success && int.TryParse(yearMatch.Value, out int year))
        {
            driver.CarYear = year;
        }

        // Extract Color (Common colors)
        var colors = new[] { "black", "white", "silver", "gray", "red", "blue", "green", "yellow" };
        foreach (var color in colors)
        {
            if (transcript.Contains(color, StringComparison.OrdinalIgnoreCase))
            {
                driver.CarColor = char.ToUpper(color[0]) + color.Substring(1);
                break;
            }
        }

        // Extract Make (Common makes)
        var makes = new[] { "BMW", "Audi", "Tesla", "Toyota", "Honda", "Ford", "Chevrolet", "Mercedes", "Lexus", "Nissan" };
        foreach (var make in makes)
        {
            if (transcript.Contains(make, StringComparison.OrdinalIgnoreCase))
            {
                driver.CarMake = make;
                // Simple assumption: Next word might be model
                driver.CarModel = "Unknown Model"; 
                break;
            }
        }

        // Extract Violation
        if (transcript.Contains("turn", StringComparison.OrdinalIgnoreCase))
        {
            driver.ViolationType = "Turned without signal";
        }
        else if (transcript.Contains("lane", StringComparison.OrdinalIgnoreCase))
        {
            driver.ViolationType = "Changed lane without signal";
        }
        else
        {
            driver.ViolationType = "General Violation";
        }

        // Location extraction (everything after 'at')
        var atIndex = transcript.LastIndexOf(" at ", StringComparison.OrdinalIgnoreCase);
        if (atIndex != -1)
        {
            driver.Location = transcript.Substring(atIndex + 4).Trim();
        }

        // Defaults
        if (string.IsNullOrEmpty(driver.CarMake)) driver.CarMake = "Generic";
        if (string.IsNullOrEmpty(driver.CarModel)) driver.CarModel = "Car";
        if (string.IsNullOrEmpty(driver.CarColor)) driver.CarColor = "Unknown";

        return driver;
    }

    // Helper class for JSON deserialization
    private class AIResponseModel
    {
        public string? CarMake { get; set; }
        public string? CarModel { get; set; }
        public string? CarColor { get; set; }
        public int? CarYear { get; set; }
        public string? ViolationType { get; set; }
        public string? Location { get; set; }
    }
}
