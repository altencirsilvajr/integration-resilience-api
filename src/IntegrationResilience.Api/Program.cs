using IntegrationResilience.Application;
using IntegrationResilience.Contracts;
using IntegrationResilience.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddCustomerScoreIntegration(builder.Configuration);
builder.Services.AddSingleton<ScoreLookupService>();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins("http://localhost:5406").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseExceptionHandler();
app.UseCors();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).WithName("Health");

var scores = app.MapGroup("/api/customers").WithTags("Customer scores");
scores.MapGet("/{customerId}/score", async (
    string customerId,
    string scenario,
    string? correlationId,
    ScoreLookupService lookup,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var id = string.IsNullOrWhiteSpace(correlationId) ? Guid.NewGuid().ToString("N") : correlationId;
    httpContext.Response.Headers["X-Correlation-Id"] = id;
    var outcome = await lookup.GetAsync(customerId, scenario, id, cancellationToken);
    if (outcome.Response is { } response) return Results.Ok(response);

    var failure = outcome.Failure!;
    var status = failure.Class switch
    {
        "provider-timeout" => StatusCodes.Status504GatewayTimeout,
        "provider-throttled" => StatusCodes.Status429TooManyRequests,
        "circuit-open" => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status502BadGateway
    };
    return Results.Problem(
        type: $"https://httpstatuses.com/{status}#{failure.Class}",
        title: "Customer score integration failed",
        statusCode: status,
        detail: failure.Detail,
        extensions: new Dictionary<string, object?> { ["correlationId"] = id, ["failureClass"] = failure.Class });
}).WithName("GetCustomerScore").Produces<CustomerScoreResponse>().ProducesProblem(StatusCodes.Status502BadGateway);

app.MapGet("/api/resilience/state", (string correlationId, IResilienceObservability observability) => Results.Ok(observability.Read(correlationId)))
    .WithTags("Resilience").WithName("GetResilienceState").Produces<ResilienceStateResponse>();

app.Run();

public partial class Program;
