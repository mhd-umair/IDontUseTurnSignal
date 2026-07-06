using TurnSignalTracker.Components;
using TurnSignalTracker.Models;
using TurnSignalTracker.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers(); // For Auth Controller

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.Cookie.Name = "auth_token";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddCascadingAuthenticationState();

builder.Services.Configure<AutomationSettings>(builder.Configuration.GetSection("Automation"));

builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAutomationService, AutomationService>();
builder.Services.AddHttpClient<IAIService, AIService>();
builder.Services.AddHttpClient<IAIInsightsService, AIInsightsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // Map Auth Controller

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
