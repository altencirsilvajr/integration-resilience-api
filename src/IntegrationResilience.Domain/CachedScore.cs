namespace IntegrationResilience.Domain;

public sealed record CachedScore(string CustomerId, int Score, DateTimeOffset ObtainedAtUtc)
{
    public double AgeInSeconds(DateTimeOffset now) => Math.Max(0, (now - ObtainedAtUtc).TotalSeconds);
}
