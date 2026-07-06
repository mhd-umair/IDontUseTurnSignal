using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TurnSignalTracker.Models;

namespace TurnSignalTracker.Services;

public interface IAIInsightsService
{
    Task<string> GenerateInsightsAsync(IEnumerable<BadDriver> violations, IEnumerable<BadDriverStats> stats);
}

public class AIInsightsService : IAIInsightsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly AutomationSettings _settings;

    public AIInsightsService(
        HttpClient httpClient,
        IConfiguration configuration,
        IOptions<AutomationSettings> settings)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _settings = settings.Value;
    }

    public async Task<string> GenerateInsightsAsync(IEnumerable<BadDriver> violations, IEnumerable<BadDriverStats> stats)
    {
        if (!_settings.EnableAiInsights)
        {
            return BuildFallbackInsights(violations, stats);
        }

        var provider = _configuration["AISettings:Provider"];
        var violationList = violations.Take(10).ToList();
        var statsList = stats.Take(5).ToList();

        if (violationList.Count == 0)
        {
            return "No violations recorded yet. AI insights will appear once data is available.";
        }

        var summary = BuildDataSummary(violationList, statsList);

        try
        {
            if (string.Equals(provider, "OpenAI", StringComparison.OrdinalIgnoreCase))
            {
                return await GenerateWithOpenAI(summary);
            }

            if (string.Equals(provider, "Gemini", StringComparison.OrdinalIgnoreCase))
            {
                return await GenerateWithGemini(summary);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AI insights failed: {ex.Message}");
        }

        return BuildFallbackInsights(violationList, statsList);
    }

    private static string BuildDataSummary(IEnumerable<BadDriver> violations, IEnumerable<BadDriverStats> stats)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Total violations: {violations.Count()}");
        sb.AppendLine("Top makes: " + string.Join(", ", stats.Select(s => $"{s.CarMake} ({s.Count})")));
        sb.AppendLine("Recent violations:");
        foreach (var v in violations)
        {
            sb.AppendLine($"- {v.CarColor} {v.CarMake} {v.CarModel}: {v.ViolationType} at {v.Location}");
        }
        return sb.ToString();
    }

    private async Task<string> GenerateWithOpenAI(string dataSummary)
    {
        var apiKey = _configuration["AISettings:OpenAI:ApiKey"];
        var model = _configuration["AISettings:OpenAI:Model"] ?? "gpt-3.5-turbo";
        if (string.IsNullOrEmpty(apiKey)) throw new InvalidOperationException("OpenAI API Key missing");

        var requestBody = new
        {
            model,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = "You are a traffic safety analyst. Write 2-3 concise sentences summarizing violation trends. Be specific about makes, patterns, and hotspots. No bullet points."
                },
                new { role = "user", content = dataSummary }
            },
            temperature = 0.3
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {apiKey}");
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? BuildFallbackInsightsFromSummary(dataSummary);
    }

    private async Task<string> GenerateWithGemini(string dataSummary)
    {
        var apiKey = _configuration["AISettings:Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey)) throw new InvalidOperationException("Gemini API Key missing");

        var prompt = $"Analyze this traffic violation data and write 2-3 concise insight sentences:\n\n{dataSummary}";
        var requestBody = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}";
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? BuildFallbackInsightsFromSummary(dataSummary);
    }

    private static string BuildFallbackInsights(IEnumerable<BadDriver> violations, IEnumerable<BadDriverStats> stats)
    {
        var topMake = stats.FirstOrDefault();
        var recent = violations.FirstOrDefault();
        if (topMake == null || recent == null)
        {
            return "No violations recorded yet.";
        }

        return $"The most reported make is {topMake.CarMake} with {topMake.Count} violations. " +
               $"The latest report was a {recent.CarColor} {recent.CarMake} {recent.CarModel} for \"{recent.ViolationType}\".";
    }

    private static string BuildFallbackInsightsFromSummary(string summary)
    {
        return summary.Split('\n').FirstOrDefault(l => l.StartsWith("Top makes:"))?.Replace("Top makes: ", "Trend: ") 
            ?? "Violation data is being analyzed.";
    }
}
