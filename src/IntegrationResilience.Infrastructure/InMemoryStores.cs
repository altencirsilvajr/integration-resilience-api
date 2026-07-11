using System.Collections.Concurrent;
using IntegrationResilience.Application;
using IntegrationResilience.Contracts;
using IntegrationResilience.Domain;

namespace IntegrationResilience.Infrastructure;

public sealed class InMemoryScoreFallbackStore : IScoreFallbackStore
{
    private readonly ConcurrentDictionary<string, CachedScore> _scores = new(StringComparer.OrdinalIgnoreCase);
    public CachedScore? Get(string customerId) => _scores.TryGetValue(customerId, out var score) ? score : null;
    public void Save(CachedScore score) => _scores[score.CustomerId] = score;
}

public sealed class ResilienceObservability : IResilienceObservability
{
    private readonly ConcurrentQueue<TimelineEventContract> _events = new();
    private readonly TimeProvider _timeProvider;
    private string _circuitState = "Closed";
    private DateTimeOffset _changedAtUtc;

    public ResilienceObservability(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _changedAtUtc = timeProvider.GetUtcNow();
    }

    public void Record(string correlationId, string kind, string detail)
    {
        _events.Enqueue(new TimelineEventContract(_timeProvider.GetUtcNow(), correlationId, kind, detail));
        while (_events.Count > 160) _events.TryDequeue(out _);
    }

    public void SetCircuitState(string state, string detail)
    {
        _circuitState = state;
        _changedAtUtc = _timeProvider.GetUtcNow();
        Record("circuit", "Circuit", detail);
    }

    public ResilienceStateResponse Read(string correlationId) => new(
        _circuitState, _changedAtUtc,
        _events.Where(x => x.CorrelationId == correlationId || x.CorrelationId == "circuit").OrderBy(x => x.AtUtc).ToArray());
}
