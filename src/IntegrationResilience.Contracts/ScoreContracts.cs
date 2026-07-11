namespace IntegrationResilience.Contracts;

public sealed record CustomerScoreResponse(
    string CustomerId,
    int Score,
    string Source,
    DateTimeOffset ObtainedAtUtc,
    double? FallbackAgeSeconds,
    string CorrelationId,
    string? ProviderFailureClass);

public sealed record ProblemDetailsContract(
    string Type,
    string Title,
    int Status,
    string? Detail,
    string? CorrelationId,
    string? FailureClass);

public sealed record TimelineEventContract(
    DateTimeOffset AtUtc,
    string CorrelationId,
    string Kind,
    string Detail);

public sealed record ResilienceStateResponse(
    string CircuitState,
    DateTimeOffset ChangedAtUtc,
    IReadOnlyList<TimelineEventContract> Timeline);
