var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", role = "fake-provider" }));
app.MapGet("/provider/scores/{customerId}", async (string customerId, string scenario, HttpContext context, CancellationToken cancellationToken) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? "missing";
    app.Logger.LogInformation("Fake provider scenario {Scenario} for {CustomerId} ({CorrelationId})", scenario, customerId, correlationId);
    return scenario.ToLowerInvariant() switch
    {
        "success" => Results.Ok(new { score = 780, obtainedAtUtc = DateTimeOffset.UtcNow }),
        "timeout" => await DelayedFailure(cancellationToken),
        "rate-limit" => Results.Json(new { message = "Configured HTTP 429" }, statusCode: StatusCodes.Status429TooManyRequests),
        "server-error" => Results.Json(new { message = "Configured HTTP 503" }, statusCode: StatusCodes.Status503ServiceUnavailable),
        _ => Results.BadRequest(new { message = "Scenario must be success, timeout, rate-limit, or server-error." })
    };
});

app.Run();

static async Task<IResult> DelayedFailure(CancellationToken cancellationToken)
{
    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    return Results.Ok(new { score = 780, obtainedAtUtc = DateTimeOffset.UtcNow });
}
