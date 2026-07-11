using IntegrationResilience.Application;
using Microsoft.Extensions.Logging;

namespace IntegrationResilience.Infrastructure;

public sealed class OutboundAttemptHandler(IResilienceObservability observability, ILogger<OutboundAttemptHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = request.Headers.TryGetValues("X-Correlation-Id", out var values) ? values.Single() : "unknown";
        observability.Record(correlationId, "Attempt", $"GET {request.RequestUri}");
        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            observability.Record(correlationId, "ProviderResponse", $"HTTP {(int)response.StatusCode}");
            logger.LogInformation("Provider attempt completed: {StatusCode} {CorrelationId}", (int)response.StatusCode, correlationId);
            return response;
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            observability.Record(correlationId, "AttemptException", exception.GetType().Name);
            logger.LogWarning(exception, "Provider attempt failed for {CorrelationId}", correlationId);
            throw;
        }
    }
}
