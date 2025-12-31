using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TurnSignalViolationTracker.Core.DTOs;
using TurnSignalViolationTracker.Core.Interfaces;

namespace TurnSignalViolationTracker.Infrastructure.Services;

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeocodingService> _logger;

    public GeocodingService(HttpClient httpClient, ILogger<GeocodingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GeocodingResult> ReverseGeocodeAsync(double latitude, double longitude)
    {
        try
        {
            // Using OpenStreetMap Nominatim API (free, no API key required)
            var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}&zoom=18&addressdetails=1";
            
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TurnSignalViolationTracker/1.0");
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                return new GeocodingResult
                {
                    Success = false,
                    ErrorMessage = $"Geocoding API returned {response.StatusCode}"
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<NominatimResponse>(content, options);

            if (result == null)
            {
                return new GeocodingResult
                {
                    Success = false,
                    ErrorMessage = "Failed to parse geocoding response"
                };
            }

            return new GeocodingResult
            {
                Success = true,
                FormattedAddress = result.DisplayName,
                City = result.Address?.City ?? result.Address?.Town ?? result.Address?.Village ?? result.Address?.County,
                State = result.Address?.State,
                Country = result.Address?.Country,
                Street = result.Address?.Road
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing reverse geocoding for {Lat}, {Lon}", latitude, longitude);
            return new GeocodingResult
            {
                Success = false,
                ErrorMessage = $"Geocoding failed: {ex.Message}"
            };
        }
    }

    private class NominatimResponse
    {
        public string? DisplayName { get; set; }
        public NominatimAddress? Address { get; set; }
    }

    private class NominatimAddress
    {
        public string? Road { get; set; }
        public string? City { get; set; }
        public string? Town { get; set; }
        public string? Village { get; set; }
        public string? County { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
    }
}
