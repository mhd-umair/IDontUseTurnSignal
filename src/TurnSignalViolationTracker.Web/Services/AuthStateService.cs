namespace TurnSignalViolationTracker.Web.Services;

public class AuthStateService
{
    public bool IsAuthenticated { get; private set; }
    public int? UserId { get; private set; }
    public string? Username { get; private set; }
    public string? FullName { get; private set; }

    public event Action? OnAuthStateChanged;

    public void Login(int userId, string username, string fullName)
    {
        IsAuthenticated = true;
        UserId = userId;
        Username = username;
        FullName = fullName;
        OnAuthStateChanged?.Invoke();
    }

    public void Logout()
    {
        IsAuthenticated = false;
        UserId = null;
        Username = null;
        FullName = null;
        OnAuthStateChanged?.Invoke();
    }
}
