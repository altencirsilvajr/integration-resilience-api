namespace IntegrationResilience.Infrastructure;

public sealed class ResilienceOptions
{
    public const string SectionName = "Resilience";
    public int AttemptTimeoutMilliseconds { get; init; } = 450;
    public int RetryCount { get; init; } = 2;
    public int CircuitBreakSeconds { get; init; } = 5;
}
