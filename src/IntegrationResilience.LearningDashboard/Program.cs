using IntegrationResilience.LearningDashboard.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient<LabApiClient>(client => client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5306/"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public sealed class LabApiClient(HttpClient httpClient)
{
    public async Task<LabCallResult> RunAsync(string scenario, string correlationId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/customers/demo-customer/score?scenario={Uri.EscapeDataString(scenario)}&correlationId={correlationId}", cancellationToken);
        return new LabCallResult((int)response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
    }

    public Task<string> GetStateAsync(string correlationId, CancellationToken cancellationToken = default) =>
        httpClient.GetStringAsync($"api/resilience/state?correlationId={correlationId}", cancellationToken);
}

public sealed record LabCallResult(int StatusCode, string Body);
