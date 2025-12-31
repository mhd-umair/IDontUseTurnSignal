using TurnSignalViolationTracker.Core.DTOs;

namespace TurnSignalViolationTracker.Core.Interfaces;

public interface IGeocodingService
{
    Task<GeocodingResult> ReverseGeocodeAsync(double latitude, double longitude);
}
