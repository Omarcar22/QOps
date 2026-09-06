namespace QOps.Domain.Pipelines;

public enum PipelineExecutionStatus
{
    Queued,
    Running,
    Succeeded,
    Failed,
    Cancelled
}

public sealed class PipelineExecution
{
    private PipelineExecution()
    {
    }

    public PipelineExecution(
        Guid projectId,
        Guid pipelineId,
        string triggeredBy,
        string? notes = null,
        PipelineExecutionStatus status = PipelineExecutionStatus.Queued)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project ID is required.", nameof(projectId));
        }

        if (pipelineId == Guid.Empty)
        {
            throw new ArgumentException("Pipeline ID is required.", nameof(pipelineId));
        }

        if (string.IsNullOrWhiteSpace(triggeredBy))
        {
            throw new ArgumentException("Triggered by is required.", nameof(triggeredBy));
        }

        if (triggeredBy.Length > 120)
        {
            throw new ArgumentException("Triggered by cannot exceed 120 characters.", nameof(triggeredBy));
        }

        if (notes?.Length > 2000)
        {
            throw new ArgumentException("Execution notes cannot exceed 2000 characters.", nameof(notes));
        }

        Id = Guid.NewGuid();
        ProjectId = projectId;
        PipelineId = pipelineId;
        TriggeredBy = triggeredBy.Trim();
        Notes = notes?.Trim();
        Status = status;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public Guid PipelineId { get; private set; }

    public string TriggeredBy { get; private set; } = string.Empty;

    public string? Notes { get; private set; }

    public PipelineExecutionStatus Status { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? FinishedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Start()
    {
        if (Status == PipelineExecutionStatus.Queued)
        {
            Status = PipelineExecutionStatus.Running;
            StartedAt ??= DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void Complete(PipelineExecutionStatus status)
    {
        if (status is PipelineExecutionStatus.Queued or PipelineExecutionStatus.Running)
        {
            throw new ArgumentException("Execution must finish in a terminal state.", nameof(status));
        }

        Status = status;
        FinishedAt ??= DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
