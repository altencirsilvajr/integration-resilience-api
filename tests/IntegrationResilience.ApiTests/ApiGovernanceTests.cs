using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationResilience.ApiTests;

public sealed class ApiGovernanceTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Health_endpoint_is_available_through_the_real_http_pipeline()
    {
        var response = await factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public void Vision_and_all_required_adrs_exist()
    {
        var root = FindRepositoryRoot();
        Assert.True(File.Exists(Path.Combine(root, "PROJECT_VISION.md")));
        foreach (var number in Enumerable.Range(1, 5))
        {
            Assert.Single(Directory.GetFiles(Path.Combine(root, "docs", "adr"), $"ADR-{number:0000}-*.md"));
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "IntegrationResilience.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
