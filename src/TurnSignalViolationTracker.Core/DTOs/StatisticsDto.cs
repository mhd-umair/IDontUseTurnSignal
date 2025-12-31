namespace TurnSignalViolationTracker.Core.DTOs;

public class DashboardStatistics
{
    public int TotalViolations { get; set; }
    public int ViolationsToday { get; set; }
    public int ViolationsThisWeek { get; set; }
    public int ViolationsThisMonth { get; set; }
    public List<ViolationsByBrandDto> ViolationsByBrand { get; set; } = new();
    public List<ViolationsByColorDto> ViolationsByColor { get; set; } = new();
    public List<ViolationsByTypeDto> ViolationsByType { get; set; } = new();
    public List<ViolationsByTimeDto> ViolationsByHour { get; set; } = new();
    public List<ViolationsByDayDto> ViolationsByDayOfWeek { get; set; } = new();
    public List<ViolationsTrendDto> ViolationsTrend { get; set; } = new();
    public List<ViolationLocationDto> ViolationLocations { get; set; } = new();
}

public class ViolationsByBrandDto
{
    public string Brand { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class ViolationsByColorDto
{
    public string Color { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class ViolationsByTypeDto
{
    public string ViolationType { get; set; } = string.Empty;
    public int ViolationTypeId { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class ViolationsByTimeDto
{
    public int Hour { get; set; }
    public int Count { get; set; }
}

public class ViolationsByDayDto
{
    public string DayOfWeek { get; set; } = string.Empty;
    public int DayNumber { get; set; }
    public int Count { get; set; }
}

public class ViolationsTrendDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

public class ViolationLocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string LocationDescription { get; set; } = string.Empty;
    public int Count { get; set; }
}
