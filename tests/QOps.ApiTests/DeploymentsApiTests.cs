using System.Net;
using System.Net.Http.Json;
using QOps.Application.Deployments;
using QOps.Application.Environments;
using QOps.Application.Projects;
using QOps.Domain.Deployments;
using QOps.Domain.Environments;

namespace QOps.ApiTests;

[Collection(QOpsApiCollection.Name)]
public class DeploymentsApiTests(QOpsWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateDeployment_ShouldPersistAndReturnDeployment()
    {
        var (projectId, environmentId) = await CreateEnvironmentAsync();

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/environments/{environmentId}/deployments",
            new CreateDeploymentRequest("1.2.0", "Initial release"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<DeploymentResponse>();
        Assert.NotNull(created);
        Assert.Equal(projectId, created.ProjectId);
        Assert.Equal(environmentId, created.EnvironmentId);
        Assert.Equal("1.2.0", created.Version);
        Assert.Equal(DeploymentStatus.Pending, created.Status);

        var getResponse = await _client.GetAsync(
            $"/api/projects/{projectId}/environments/{environmentId}/deployments/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetDeployments_ShouldReturnDeploymentsForEnvironment()
    {
        var (projectId, environmentId) = await CreateEnvironmentAsync();
        var createResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/environments/{environmentId}/deployments",
            new CreateDeploymentRequest("2.0.0", null));
        var created = await createResponse.Content.ReadFromJsonAsync<DeploymentResponse>();
        Assert.NotNull(created);

        var listResponse = await _client.GetAsync(
            $"/api/projects/{projectId}/environments/{environmentId}/deployments");
        var deployments = await listResponse.Content.ReadFromJsonAsync<DeploymentResponse[]>();

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.NotNull(deployments);
        Assert.Contains(deployments, deployment => deployment.Id == created.Id);
    }

    [Fact]
    public async Task UpdateDeployment_ShouldRecordSuccessfulDeployment()
    {
        var (projectId, environmentId) = await CreateEnvironmentAsync();
        var createResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/environments/{environmentId}/deployments",
            new CreateDeploymentRequest("1.0.0", null));
        var created = await createResponse.Content.ReadFromJsonAsync<DeploymentResponse>();
        Assert.NotNull(created);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/projects/{projectId}/environments/{environmentId}/deployments/{created.Id}",
            new UpdateDeploymentRequest("1.1.0", "Released to production", DeploymentStatus.Succeeded));
        var updated = await updateResponse.Content.ReadFromJsonAsync<DeploymentResponse>();

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal("1.1.0", updated.Version);
        Assert.Equal(DeploymentStatus.Succeeded, updated.Status);
        Assert.NotNull(updated.DeployedAt);
    }

    [Fact]
    public async Task DeleteDeployment_ShouldReturnNoContentAndThenNotFound()
    {
        var (projectId, environmentId) = await CreateEnvironmentAsync();
        var createResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/environments/{environmentId}/deployments",
            new CreateDeploymentRequest("3.0.0", null));
        var created = await createResponse.Content.ReadFromJsonAsync<DeploymentResponse>();
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync(
            $"/api/projects/{projectId}/environments/{environmentId}/deployments/{created.Id}");
        var getResponse = await _client.GetAsync(
            $"/api/projects/{projectId}/environments/{environmentId}/deployments/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private async Task<(Guid ProjectId, Guid EnvironmentId)> CreateEnvironmentAsync()
    {
        var projectResponse = await _client.PostAsJsonAsync(
            "/api/projects",
            new CreateProjectRequest($"Deployment project {Guid.NewGuid():N}", null, "Development", "1.0.0"));
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(project);

        var environmentResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{project.Id}/environments",
            new CreateEnvironmentRequest($"Development {Guid.NewGuid():N}", EnvironmentType.Development, "https://dev.example.com"));
        var environment = await environmentResponse.Content.ReadFromJsonAsync<EnvironmentResponse>();
        Assert.NotNull(environment);

        return (project.Id, environment.Id);
    }
}
