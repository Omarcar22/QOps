using QOps.Domain.Pipelines;

namespace QOps.Application.Pipelines;

public sealed record CreatePipelineStepRequest(
    string Name,
    PipelineStepType Type,
    int Order,
    string? Configuration);

public sealed record CreatePipelineRequest(
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyCollection<CreatePipelineStepRequest> Steps);

public sealed record UpdatePipelineRequest(
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyCollection<CreatePipelineStepRequest> Steps);

public sealed record PipelineStepResponse(
    Guid Id,
    Guid PipelineId,
    string Name,
    PipelineStepType Type,
    int Order,
    string? Configuration);

public sealed record PipelineResponse(
    Guid Id,
    Guid ProjectId,
    string Name,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<PipelineStepResponse> Steps);

public sealed record CreatePipelineExecutionRequest(
    string TriggeredBy,
    string? Notes = null);

public sealed record PipelineExecutionResponse(
    Guid Id,
    Guid ProjectId,
    Guid PipelineId,
    string TriggeredBy,
    string? Notes,
    PipelineExecutionStatus Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public interface IPipelineService
{
    Task<PipelineResponse> CreateAsync(Guid projectId, CreatePipelineRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PipelineResponse>> GetAllAsync(Guid projectId, CancellationToken cancellationToken);

    Task<PipelineResponse?> GetByIdAsync(Guid projectId, Guid id, CancellationToken cancellationToken);

    Task<PipelineResponse?> UpdateAsync(Guid projectId, Guid id, UpdatePipelineRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid projectId, Guid id, CancellationToken cancellationToken);

    Task<PipelineExecutionResponse> CreateExecutionAsync(Guid projectId, Guid pipelineId, CreatePipelineExecutionRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PipelineExecutionResponse>> GetExecutionsAsync(Guid projectId, Guid pipelineId, CancellationToken cancellationToken);
}

public interface IPipelineRepository
{
    Task AddAsync(Pipeline pipeline, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Pipeline>> GetAllAsync(Guid projectId, CancellationToken cancellationToken);

    Task<Pipeline?> GetByIdAsync(Guid projectId, Guid id, CancellationToken cancellationToken);

    Task AddExecutionAsync(PipelineExecution execution, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PipelineExecution>> GetExecutionsAsync(Guid projectId, Guid pipelineId, CancellationToken cancellationToken);

    void Remove(Pipeline pipeline);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
