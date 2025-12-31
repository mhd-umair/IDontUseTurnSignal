using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TurnSignalViolationTracker.Core.Enums;
using TurnSignalViolationTracker.Core.Interfaces;
using TurnSignalViolationTracker.Infrastructure.Configuration;

namespace TurnSignalViolationTracker.Infrastructure.Services;

public class AIServiceFactory : IAIServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AISettings _settings;

    public AIServiceFactory(IServiceProvider serviceProvider, IOptions<AISettings> settings)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
    }

    public IAIService CreateAIService()
    {
        return _settings.Provider switch
        {
            AIProvider.OpenAI => _serviceProvider.GetRequiredService<OpenAIService>(),
            AIProvider.Gemini => _serviceProvider.GetRequiredService<GeminiAIService>(),
            _ => throw new ArgumentException($"Unknown AI provider: {_settings.Provider}")
        };
    }
}
