using IntegrationResilience.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationResilience.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerScoreIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(ResilienceOptions.SectionName).Get<ResilienceOptions>() ?? new ResilienceOptions();
        services.AddSingleton(options);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IScoreFallbackStore, InMemoryScoreFallbackStore>();
        services.AddSingleton<IResilienceObservability, ResilienceObservability>();
        services.AddTransient<OutboundAttemptHandler>();
        services.AddHttpClient<ICustomerScoreGateway, CustomerScoreProviderClient>((_, client) =>
        {
            client.BaseAddress = new Uri(configuration["Provider:BaseUrl"] ?? "http://localhost:5307/");
            client.Timeout = Timeout.InfiniteTimeSpan;
        }).AddHttpMessageHandler<OutboundAttemptHandler>();
        return services;
    }
}
