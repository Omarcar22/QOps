using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace QOps.ApiTests;

public class HealthApiTests
{
    private readonly HttpClient _client;

    public HealthApiTests()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var baseUrl = configuration["ApiSettings:BaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "ApiSettings:BaseUrl no está configurado.");
        }

        _client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    [Fact]
    public async Task GetHealth_ShouldReturnHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content
            .ReadFromJsonAsync<HealthResponse>();

        Assert.NotNull(content);
        Assert.Equal("Healthy", content.Status);
        Assert.Equal("QOps", content.Application);
        Assert.Equal("1.0.0", content.Version);
    }

    private record HealthResponse(
        string Status,
        string Application,
        string Version);
}