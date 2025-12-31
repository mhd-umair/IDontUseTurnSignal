using System.Text.RegularExpressions;
using TurnSignalTracker.Models;

namespace TurnSignalTracker.Services;

public interface IAIService
{
    Task<BadDriver> ProcessVoiceLogAsync(string transcript, double? lat, double? lon);
}

public class AIService : IAIService
{
    // In a real app, this would call Azure OpenAI or ChatGPT API
    public async Task<BadDriver> ProcessVoiceLogAsync(string transcript, double? lat, double? lon)
    {
        // Simulate AI delay
        await Task.Delay(1000);

        var driver = new BadDriver
        {
            Latitude = lat,
            Longitude = lon,
            Location = "Unknown Location", // Placeholder until reverse geocoding
            CreatedAt = DateTime.Now
        };

        // Simulated Reverse Geocoding if GPS is available
        if (lat.HasValue && lon.HasValue)
        {
             // In production: await ReverseGeocodeAsync(lat.Value, lon.Value);
             driver.Location = $"{lat.Value:F4}, {lon.Value:F4}";
        }

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
                // Simple assumption: Next word might be model if it's not a color or year
                // This is very basic, a real LLM handles this perfectly.
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

        // Fallback for demo if parsing fails (so the user sees something)
        if (string.IsNullOrEmpty(driver.CarMake)) driver.CarMake = "Generic";
        if (string.IsNullOrEmpty(driver.CarModel)) driver.CarModel = "Car";
        if (string.IsNullOrEmpty(driver.CarColor)) driver.CarColor = "Unknown";

        return driver;
    }
}
