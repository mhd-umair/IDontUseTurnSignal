namespace TurnSignalViolationTracker.Core.DTOs;

public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string? FullName { get; set; }
}
