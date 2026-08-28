using System.Net;
using System.Net.Http.Json;
using QOps.Application.Projects;

namespace QOps.ApiTests;

[Collection(QOpsApiCollection.Name)]
public class ProjectsApiTests(QOpsWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateProject_ShouldPersistAndReturnProject()
    {
        var request = new CreateProjectRequest(
            $"Integration project {Guid.NewGuid():N}",
            "Created by integration test",
            "Test",
            "1.0.0");

        var createResponse = await _client.PostAsJsonAsync("/api/projects", request);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(created);

        var getResponse = await _client.GetAsync($"/api/projects/{created.Id}");
        var persisted = await getResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(persisted);
        Assert.Equal(created.Id, persisted.Id);
        Assert.Equal(request.Name, persisted.Name);
    }

    [Fact]
    public async Task GetProject_WithUnknownId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/projects/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProjects_ShouldIncludeCreatedProject()
    {
        var request = new CreateProjectRequest(
            $"List project {Guid.NewGuid():N}",
            null,
            "Development",
            "1.0.0");

        var createResponse = await _client.PostAsJsonAsync("/api/projects", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var listResponse = await _client.GetAsync("/api/projects");
        var projects = await listResponse.Content.ReadFromJsonAsync<ProjectResponse[]>();

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.NotNull(created);
        Assert.NotNull(projects);
        Assert.Contains(projects, project => project.Id == created.Id);
    }

    [Fact]
    public async Task UpdateProject_ShouldReturnUpdatedValues()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/projects",
            new CreateProjectRequest(
                $"Update project {Guid.NewGuid():N}",
                "Before update",
                "Development",
                "1.0.0"));
        var created = await createResponse.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(created);

        var updateRequest = new UpdateProjectRequest(
            created.Name,
            "After update",
            "Production",
            "2.0.0",
            QOps.Domain.Projects.ProjectStatus.Archived);
        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/projects/{created.Id}",
            updateRequest);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal("After update", updated.Description);
        Assert.Equal("Production", updated.Environment);
        Assert.Equal("2.0.0", updated.Version);
        Assert.Equal(QOps.Domain.Projects.ProjectStatus.Archived, updated.Status);
    }

    [Fact]
    public async Task DeleteProject_ShouldReturnNoContentAndThenNotFound()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/projects",
            new CreateProjectRequest(
                $"Delete project {Guid.NewGuid():N}",
                null,
                "Test",
                "1.0.0"));
        var created = await createResponse.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/projects/{created.Id}");
        var getResponse = await _client.GetAsync($"/api/projects/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateProject_WithBlankName_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/projects",
            new CreateProjectRequest(" ", null, "Test", "1.0.0"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}