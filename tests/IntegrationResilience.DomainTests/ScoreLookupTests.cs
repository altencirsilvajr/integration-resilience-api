using IntegrationResilience.Application;
using IntegrationResilience.Domain;

namespace IntegrationResilience.DomainTests;

public sealed class ScoreLookupTests
{
    [Fact]
    public void Cached_score_reports_non_negative_age()
    {
        var cached = new CachedScore("ada", 720, DateTimeOffset.Parse("2026-07-11T10:00:00Z"));

        Assert.Equal(0, cached.AgeInSeconds(DateTimeOffset.Parse("2026-07-11T09:59:00Z")));
        Assert.Equal(60, cached.AgeInSeconds(DateTimeOffset.Parse("2026-07-11T10:01:00Z")));
    }

    [Fact]
    public async Task Integration_failure_returns_explicitly_marked_fallback()
    {
        var store = new Store();
        store.Save(new CachedScore("ada", 720, DateTimeOffset.Parse("2026-07-11T10:00:00Z")));
        var service = new ScoreLookupService(new FailingGateway(), store, new FixedTimeProvider(DateTimeOffset.Parse("2026-07-11T10:02:00Z")));

        var outcome = await service.GetAsync("ada", "server-error", "correlation", CancellationToken.None);

        Assert.True(outcome.IsFallback);
        Assert.Equal("fallback", outcome.Response!.Source);
        Assert.Equal("provider-unavailable", outcome.Response.ProviderFailureClass);
        Assert.Equal(120, outcome.Response.FallbackAgeSeconds);
    }

    private sealed class FailingGateway : ICustomerScoreGateway
    {
        public Task<ExternalScoreResult> GetScoreAsync(string customerId, string scenario, string correlationId, CancellationToken cancellationToken) =>
            Task.FromResult(ExternalScoreResult.Failed("provider-unavailable", "configured failure"));
    }

    private sealed class Store : IScoreFallbackStore
    {
        private CachedScore? _score;
        public CachedScore? Get(string customerId) => _score;
        public void Save(CachedScore score) => _score = score;
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
