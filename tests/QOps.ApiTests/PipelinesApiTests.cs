using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QOps.Application.Pipelines;
using QOps.Application.Projects;
using QOps.Domain.Pipelines;

namespace QOps.ApiTests;

[Collection(QOpsApiCollection.Name)]
public class PipelinesApiTests(QOpsWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreatePipeline_ShouldPersistAndReturnPipeline()
    {
        var projectResponse = await _client.PostAsJsonAsync(
            "/api/projects",
            new
            {
                name = $"Pipeline project {Guid.NewGuid():N}",
                description = "Project for pipeline tests",
                environment = "Test",
                version = "1.0.0"
            });

        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(project);

        var request = new CreatePipelineRequest(
            "Build and deploy",
            "Main release pipeline",
            true,
            [
                new CreatePipelineStepRequest("Build", PipelineStepType.Build, 1, "dotnet build"),
                new CreatePipelineStepRequest("Deploy", PipelineStepType.Deploy, 2, "kubectl apply")
            ]);

        var createResponse = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/pipelines", request);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<PipelineResponse>();
        Assert.NotNull(created);
        Assert.Equal(request.Name, created.Name);
        Assert.Equal(2, created.Steps.Count);

        var getResponse = await _client.GetAsync($"/api/projects/{project.Id}/pipelines/{created.Id}");
        var persisted = await getResponse.Content.ReadFromJsonAsync<PipelineResponse>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(persisted);
        Assert.Equal(created.Id, persisted.Id);
        Assert.Equal(request.Name, persisted.Name);
    }

    [Fact]
    public async Task CreatePipeline_WithBlankName_ShouldReturnBadRequest()
    {
        var projectResponse = await _client.PostAsJsonAsync(
            "/api/projects",
            new
            {
                name = $"Blank pipeline project {Guid.NewGuid():N}",
                description = "Project for validation tests",
                environment = "Test",
                version = "1.0.0"
            });

        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(project);

        var response = await _client.PostAsJsonAsync(
            $"/api/projects/{project.Id}/pipelines",
            new CreatePipelineRequest(" ", null, true, []));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePipelineExecution_ShouldReturnCreatedAndPersistRun()
    {
        var projectResponse = await _client.PostAsJsonAsync(
            "/api/projects",
            new
            {
                name = $"Execution project {Guid.NewGuid():N}",
                description = "Project for pipeline execution tests",
                environment = "Test",
                version = "1.0.0"
            });

        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(project);

        var createPipelineResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{project.Id}/pipelines",
            new CreatePipelineRequest(
                "Validation pipeline",
                "Runs build and Playwright checks",
                true,
                [
                    new CreatePipelineStepRequest("Build", PipelineStepType.Build, 1, "dotnet build"),
                    new CreatePipelineStepRequest("Playwright", PipelineStepType.Playwright, 2, "npx playwright test")
                ]));

        var pipeline = await createPipelineResponse.Content.ReadFromJsonAsync<PipelineResponse>();
        Assert.NotNull(pipeline);

        var executionResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{project.Id}/pipelines/{pipeline.Id}/executions",
            new { triggeredBy = "qa-automation" });

        Assert.Equal(HttpStatusCode.Created, executionResponse.StatusCode);

        var listResponse = await _client.GetAsync($"/api/projects/{project.Id}/pipelines/{pipeline.Id}/executions");
        var document = await listResponse.Content.ReadFromJsonAsync<JsonElement[]>();

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.NotNull(document);
        Assert.Single(document);
    }
}
