using System.Net;
using System.Net.Http.Json;
using QOps.Application.Projects;
using QOps.Application.Releases;
using QOps.Domain.Releases;

namespace QOps.ApiTests;

[Collection(QOpsApiCollection.Name)]
public class ReleasesApiTests(QOpsWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateRelease_ShouldPersistAndReturnRelease()
    {
        var projectId = await CreateProjectAsync();
        var createResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/releases",
            new CreateReleaseRequest("1.0.0", "First release", "abc123"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ReleaseResponse>();
        Assert.NotNull(created);
        Assert.Equal(projectId, created.ProjectId);
        Assert.Equal("1.0.0", created.Version);
        Assert.Equal(ReleaseStatus.Draft, created.Status);
    }

    [Fact]
    public async Task GetReleases_ShouldIncludeCreatedRelease()
    {
        var projectId = await CreateProjectAsync();
        var createResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/releases",
            new CreateReleaseRequest("2.0.0", null, null));
        var created = await createResponse.Content.ReadFromJsonAsync<ReleaseResponse>();
        Assert.NotNull(created);

        var listResponse = await _client.GetAsync($"/api/projects/{projectId}/releases");
        var releases = await listResponse.Content.ReadFromJsonAsync<ReleaseResponse[]>();

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.NotNull(releases);
        Assert.Contains(releases, release => release.Id == created.Id);
    }

    [Fact]
    public async Task UpdateRelease_ShouldRecordPublishedAt()
    {
        var projectId = await CreateProjectAsync();
        var createResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/releases",
            new CreateReleaseRequest("3.0.0", "Ready to publish", "def456"));
        var created = await createResponse.Content.ReadFromJsonAsync<ReleaseResponse>();
        Assert.NotNull(created);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/projects/{projectId}/releases/{created.Id}",
            new UpdateReleaseRequest("3.0.0", "Published release", "def456", ReleaseStatus.Published));
        var updated = await updateResponse.Content.ReadFromJsonAsync<ReleaseResponse>();

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(ReleaseStatus.Published, updated.Status);
        Assert.NotNull(updated.PublishedAt);
    }

    [Fact]
    public async Task DeleteRelease_ShouldReturnNoContentAndThenNotFound()
    {
        var projectId = await CreateProjectAsync();
        var createResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/releases",
            new CreateReleaseRequest("4.0.0", null, null));
        var created = await createResponse.Content.ReadFromJsonAsync<ReleaseResponse>();
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/projects/{projectId}/releases/{created.Id}");
        var getResponse = await _client.GetAsync($"/api/projects/{projectId}/releases/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private async Task<Guid> CreateProjectAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/projects",
            new CreateProjectRequest($"Release project {Guid.NewGuid():N}", null, "Development", "1.0.0"));
        var project = await response.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(project);
        return project.Id;
    }
}
