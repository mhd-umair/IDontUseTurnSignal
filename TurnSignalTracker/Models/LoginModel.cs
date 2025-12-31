using System.ComponentModel.DataAnnotations;

namespace TurnSignalTracker.Models;

public class LoginModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
    
    public string? ReturnUrl { get; set; }
}
