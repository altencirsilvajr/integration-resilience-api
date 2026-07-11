using System.Net;
using System.Net.Http.Json;
using IntegrationResilience.Application;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace IntegrationResilience.Infrastructure;

public sealed class CustomerScoreProviderClient : ICustomerScoreGateway
{
    private readonly HttpClient _httpClient;
    private readonly IResilienceObservability _observability;
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    public CustomerScoreProviderClient(HttpClient httpClient, IResilienceObservability observability, ResilienceOptions options)
    {
        _httpClient = httpClient;
        _observability = observability;
        var predicate = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .Handle<TimeoutRejectedException>()
            .HandleResult(response => response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500);

        _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = predicate,
                MaxRetryAttempts = options.RetryCount,
                Delay = TimeSpan.FromMilliseconds(120),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = false,
                OnRetry = args =>
                {
                    var id = args.Context.Properties.TryGetValue(new ResiliencePropertyKey<string>("correlation-id"), out var value) ? value : "unknown";
                    _observability.Record(id, "Retry", $"retry {args.AttemptNumber + 1} after {args.RetryDelay.TotalMilliseconds:0}ms");
                    return ValueTask.CompletedTask;
                }
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = predicate,
                FailureRatio = 0.5,
                MinimumThroughput = 2,
                SamplingDuration = TimeSpan.FromSeconds(10),
                BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakSeconds),
                OnOpened = _ => { _observability.SetCircuitState("Open", "circuit opened after transient failures"); return ValueTask.CompletedTask; },
                OnHalfOpened = _ => { _observability.SetCircuitState("HalfOpen", "break duration elapsed; allowing a probe"); return ValueTask.CompletedTask; },
                OnClosed = _ => { _observability.SetCircuitState("Closed", "provider probe succeeded; circuit closed"); return ValueTask.CompletedTask; }
            })
            .AddTimeout(new TimeoutStrategyOptions { Timeout = TimeSpan.FromMilliseconds(options.AttemptTimeoutMilliseconds) })
            .Build();
    }

    public async Task<ExternalScoreResult> GetScoreAsync(string customerId, string scenario, string correlationId, CancellationToken cancellationToken)
    {
        var context = ResilienceContextPool.Shared.Get(cancellationToken);
        context.Properties.Set(new ResiliencePropertyKey<string>("correlation-id"), correlationId);
        try
        {
            var response = await _pipeline.ExecuteAsync(async ctx =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"provider/scores/{Uri.EscapeDataString(customerId)}?scenario={Uri.EscapeDataString(scenario)}");
                request.Headers.Add("X-Correlation-Id", correlationId);
                return await _httpClient.SendAsync(request, ctx.CancellationToken);
            }, context);

            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadFromJsonAsync<ProviderScorePayload>(cancellationToken: cancellationToken);
                return payload is null
                    ? ExternalScoreResult.Failed("invalid-provider-payload", "Provider returned an empty payload.")
                    : ExternalScoreResult.Success(payload.Score, payload.ObtainedAtUtc);
            }

            return response.StatusCode == HttpStatusCode.TooManyRequests
                ? ExternalScoreResult.Failed("provider-throttled", "Provider kept returning HTTP 429 after retries.")
                : ExternalScoreResult.Failed("provider-unavailable", $"Provider returned HTTP {(int)response.StatusCode} after retries.");
        }
        catch (BrokenCircuitException)
        {
            return ExternalScoreResult.Failed("circuit-open", "Circuit breaker is open; the provider was not called.");
        }
        catch (TimeoutRejectedException)
        {
            return ExternalScoreResult.Failed("provider-timeout", "Every provider attempt exceeded the configured timeout.");
        }
        catch (HttpRequestException)
        {
            return ExternalScoreResult.Failed("provider-network-error", "Provider connection could not be established.");
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
    }

    private sealed record ProviderScorePayload(int Score, DateTimeOffset ObtainedAtUtc);
}
