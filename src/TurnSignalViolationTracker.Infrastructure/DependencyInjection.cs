using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TurnSignalViolationTracker.Core.Interfaces;
using TurnSignalViolationTracker.Infrastructure.Configuration;
using TurnSignalViolationTracker.Infrastructure.Data;
using TurnSignalViolationTracker.Infrastructure.Repositories;
using TurnSignalViolationTracker.Infrastructure.Services;

namespace TurnSignalViolationTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.Configure<DatabaseSettings>(configuration.GetSection("Database"));
        services.Configure<AISettings>(configuration.GetSection("AI"));

        // Database
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICarBrandRepository, CarBrandRepository>();
        services.AddScoped<IViolationRepository, ViolationRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<OpenAIService>();
        services.AddScoped<GeminiAIService>();
        services.AddScoped<IAIServiceFactory, AIServiceFactory>();
        
        // HTTP Client for geocoding
        services.AddHttpClient<IGeocodingService, GeocodingService>();

        return services;
    }
}
