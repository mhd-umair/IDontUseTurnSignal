using System;

namespace TurnSignalTracker.Models;

public class BadDriver
{
    public int Id { get; set; }
    public string CarMake { get; set; } = string.Empty;
    public string CarModel { get; set; } = string.Empty;
    public string CarColor { get; set; } = string.Empty;
    public int? CarYear { get; set; }
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string ViolationType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
