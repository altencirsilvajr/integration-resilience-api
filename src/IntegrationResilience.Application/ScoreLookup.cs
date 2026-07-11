using IntegrationResilience.Contracts;
using IntegrationResilience.Domain;

namespace IntegrationResilience.Application;

public sealed record ExternalScore(int Value, DateTimeOffset ObtainedAtUtc);

public sealed record IntegrationFailure(string Class, string Detail);

public sealed record ExternalScoreResult(ExternalScore? Score, IntegrationFailure? Failure)
{
    public static ExternalScoreResult Success(int value, DateTimeOffset obtainedAtUtc) => new(new ExternalScore(value, obtainedAtUtc), null);
    public static ExternalScoreResult Failed(string failureClass, string detail) => new(null, new IntegrationFailure(failureClass, detail));
}

public interface ICustomerScoreGateway
{
    Task<ExternalScoreResult> GetScoreAsync(string customerId, string scenario, string correlationId, CancellationToken cancellationToken);
}

public interface IScoreFallbackStore
{
    CachedScore? Get(string customerId);
    void Save(CachedScore score);
}

public sealed class ScoreLookupService(ICustomerScoreGateway gateway, IScoreFallbackStore fallbackStore, TimeProvider timeProvider)
{
    public async Task<ScoreLookupOutcome> GetAsync(string customerId, string scenario, string correlationId, CancellationToken cancellationToken)
    {
        var external = await gateway.GetScoreAsync(customerId, scenario, correlationId, cancellationToken);
        if (external.Score is { } score)
        {
            fallbackStore.Save(new CachedScore(customerId, score.Value, score.ObtainedAtUtc));
            return ScoreLookupOutcome.Fresh(new CustomerScoreResponse(customerId, score.Value, "provider", score.ObtainedAtUtc, null, correlationId, null));
        }

        var failure = external.Failure!;
        var cached = fallbackStore.Get(customerId);
        if (cached is not null)
        {
            return ScoreLookupOutcome.Fallback(new CustomerScoreResponse(
                cached.CustomerId, cached.Score, "fallback", cached.ObtainedAtUtc,
                cached.AgeInSeconds(timeProvider.GetUtcNow()), correlationId, failure.Class));
        }

        return ScoreLookupOutcome.Failed(failure);
    }
}

public sealed record ScoreLookupOutcome(CustomerScoreResponse? Response, IntegrationFailure? Failure, bool IsFallback)
{
    public static ScoreLookupOutcome Fresh(CustomerScoreResponse response) => new(response, null, false);
    public static ScoreLookupOutcome Fallback(CustomerScoreResponse response) => new(response, null, true);
    public static ScoreLookupOutcome Failed(IntegrationFailure failure) => new(null, failure, false);
}
