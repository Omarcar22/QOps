using System.Net;
using System.Net.Http.Json;
namespace QOps.ApiTests;

 [Collection(QOpsApiCollection.Name)]
public class HealthApiTests(QOpsWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetHealth_ShouldReturnHealthyStatus()
    {
        var response = await _client.GetAsync("/api/health");

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