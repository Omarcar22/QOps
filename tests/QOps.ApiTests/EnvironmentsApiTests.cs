using System.Net;
using System.Net.Http.Json;
using QOps.Application.Environments;
using QOps.Application.Projects;
using QOps.Domain.Environments;
using QOps.Domain.Projects;

namespace QOps.ApiTests;

[Collection(QOpsApiCollection.Name)]
public class EnvironmentsApiTests(QOpsWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateEnvironment_ShouldPersistAndReturnEnvironment()
    {
        var projectResponse = await _client.PostAsJsonAsync(
            "/api/projects",
            new CreateProjectRequest(
                $"Project for environment {Guid.NewGuid():N}",
                "Project for environment tests",
                "Test",
                "1.0.0"));

        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(project);

        var environmentRequest = new CreateEnvironmentRequest(
            "Development",
            EnvironmentType.Development,
            "https://dev.example.com");

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{project.Id}/environments",
            environmentRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<EnvironmentResponse>();
        Assert.NotNull(created);
        Assert.Equal(project.Id, created.ProjectId);
        Assert.Equal("Development", created.Name);

        var getResponse = await _client.GetAsync($"/api/projects/{project.Id}/environments/{created.Id}");
        var persisted = await getResponse.Content.ReadFromJsonAsync<EnvironmentResponse>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(persisted);
        Assert.Equal(created.Id, persisted.Id);
        Assert.Equal(created.Name, persisted.Name);
    }

    [Fact]
    public async Task GetEnvironments_ShouldIncludeCreatedEnvironment()
    {
        var projectResponse = await _client.PostAsJsonAsync(
            "/api/projects",
            new CreateProjectRequest(
                $"List env project {Guid.NewGuid():N}",
                null,
                "Development",
                "1.0.0"));

        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(project);

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{project.Id}/environments",
            new CreateEnvironmentRequest(
                $"QA {Guid.NewGuid():N}",
                EnvironmentType.Staging,
                "https://qa.example.com"));

        var created = await createResponse.Content.ReadFromJsonAsync<EnvironmentResponse>();
        Assert.NotNull(created);

        var listResponse = await _client.GetAsync($"/api/projects/{project.Id}/environments");
        var environments = await listResponse.Content.ReadFromJsonAsync<EnvironmentResponse[]>();

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.NotNull(environments);
        Assert.Contains(environments, environment => environment.Id == created.Id);
    }

    [Fact]
    public async Task UpdateEnvironment_ShouldReturnUpdatedValues()
    {
        var projectResponse = await _client.PostAsJsonAsync(
            "/api/projects",
            new CreateProjectRequest(
                $"Update env project {Guid.NewGuid():N}",
                null,
                "Development",
                "1.0.0"));

        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(project);

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{project.Id}/environments",
            new CreateEnvironmentRequest(
                "Development",
                EnvironmentType.Development,
                "https://dev.example.com"));

        var created = await createResponse.Content.ReadFromJsonAsync<EnvironmentResponse>();
        Assert.NotNull(created);

        var updateRequest = new UpdateEnvironmentRequest(
            "Production",
            EnvironmentType.Production,
            "https://prod.example.com",
            EnvironmentStatus.Inactive);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/projects/{project.Id}/environments/{created.Id}",
            updateRequest);

        var updated = await updateResponse.Content.ReadFromJsonAsync<EnvironmentResponse>();

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal("Production", updated.Name);
        Assert.Equal(EnvironmentType.Production, updated.Type);
        Assert.Equal("https://prod.example.com", updated.Url);
        Assert.Equal(EnvironmentStatus.Inactive, updated.Status);
    }

    [Fact]
    public async Task DeleteEnvironment_ShouldReturnNoContentAndThenNotFound()
    {
        var projectResponse = await _client.PostAsJsonAsync(
            "/api/projects",
            new CreateProjectRequest(
                $"Delete env project {Guid.NewGuid():N}",
                null,
                "Development",
                "1.0.0"));

        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(project);

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{project.Id}/environments",
            new CreateEnvironmentRequest(
                "Staging",
                EnvironmentType.Staging,
                "https://staging.example.com"));

        var created = await createResponse.Content.ReadFromJsonAsync<EnvironmentResponse>();
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/projects/{project.Id}/environments/{created.Id}");
        var getResponse = await _client.GetAsync($"/api/projects/{project.Id}/environments/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
