using MudBlazor.Services;
using TurnSignalViolationTracker.Infrastructure;
using TurnSignalViolationTracker.Web.Components;
using TurnSignalViolationTracker.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add Infrastructure services (repositories, AI services, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Add custom services
builder.Services.AddScoped<AuthStateService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
