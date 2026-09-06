using QOps.Domain.Pipelines;

namespace QOps.Application.Pipelines;

public sealed class PipelineService(IPipelineRepository repository) : IPipelineService
{
    public async Task<PipelineResponse> CreateAsync(
        Guid projectId,
        CreatePipelineRequest request,
        CancellationToken cancellationToken)
    {
        var pipeline = new Pipeline(projectId, request.Name, request.Description, request.IsActive);

        foreach (var step in request.Steps.OrderBy(step => step.Order))
        {
            pipeline.AddStep(step.Name, step.Type, step.Order, step.Configuration);
        }

        await repository.AddAsync(pipeline, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(pipeline);
    }

    public async Task<IReadOnlyCollection<PipelineResponse>> GetAllAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var pipelines = await repository.GetAllAsync(projectId, cancellationToken);
        return pipelines.Select(Map).ToArray();
    }

    public async Task<PipelineResponse?> GetByIdAsync(
        Guid projectId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var pipeline = await repository.GetByIdAsync(projectId, id, cancellationToken);
        return pipeline is null ? null : Map(pipeline);
    }

    public async Task<PipelineResponse?> UpdateAsync(
        Guid projectId,
        Guid id,
        UpdatePipelineRequest request,
        CancellationToken cancellationToken)
    {
        var pipeline = await repository.GetByIdAsync(projectId, id, cancellationToken);

        if (pipeline is null)
        {
            return null;
        }

        pipeline.Update(request.Name, request.Description, request.IsActive);

        var existingSteps = pipeline.Steps.OrderBy(step => step.Order).ToList();
        foreach (var step in existingSteps)
        {
            pipeline.RemoveStep(step.Id);
        }

        foreach (var step in request.Steps.OrderBy(step => step.Order))
        {
            pipeline.AddStep(step.Name, step.Type, step.Order, step.Configuration);
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Map(pipeline);
    }

    public async Task<bool> DeleteAsync(Guid projectId, Guid id, CancellationToken cancellationToken)
    {
        var pipeline = await repository.GetByIdAsync(projectId, id, cancellationToken);

        if (pipeline is null)
        {
            return false;
        }

        repository.Remove(pipeline);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<PipelineExecutionResponse> CreateExecutionAsync(
        Guid projectId,
        Guid pipelineId,
        CreatePipelineExecutionRequest request,
        CancellationToken cancellationToken)
    {
        var pipeline = await repository.GetByIdAsync(projectId, pipelineId, cancellationToken);

        if (pipeline is null)
        {
            throw new KeyNotFoundException($"Pipeline {pipelineId} was not found in project {projectId}.");
        }

        var execution = new PipelineExecution(projectId, pipelineId, request.TriggeredBy, request.Notes);
        await repository.AddExecutionAsync(execution, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(execution);
    }

    public async Task<IReadOnlyCollection<PipelineExecutionResponse>> GetExecutionsAsync(
        Guid projectId,
        Guid pipelineId,
        CancellationToken cancellationToken)
    {
        var pipeline = await repository.GetByIdAsync(projectId, pipelineId, cancellationToken);

        if (pipeline is null)
        {
            return [];
        }

        var executions = await repository.GetExecutionsAsync(projectId, pipelineId, cancellationToken);
        return executions
            .OrderByDescending(execution => execution.CreatedAt)
            .Select(Map)
            .ToArray();
    }

    private static PipelineResponse Map(Pipeline pipeline) => new(
        pipeline.Id,
        pipeline.ProjectId,
        pipeline.Name,
        pipeline.Description,
        pipeline.IsActive,
        pipeline.CreatedAt,
        pipeline.UpdatedAt,
        pipeline.Steps
            .OrderBy(step => step.Order)
            .Select(step => new PipelineStepResponse(
                step.Id,
                step.PipelineId,
                step.Name,
                step.Type,
                step.Order,
                step.Configuration))
            .ToArray());

    private static PipelineExecutionResponse Map(PipelineExecution execution) => new(
        execution.Id,
        execution.ProjectId,
        execution.PipelineId,
        execution.TriggeredBy,
        execution.Notes,
        execution.Status,
        execution.StartedAt,
        execution.FinishedAt,
        execution.CreatedAt,
        execution.UpdatedAt);
}
