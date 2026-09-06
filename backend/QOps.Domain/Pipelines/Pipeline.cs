namespace QOps.Domain.Pipelines;

public enum PipelineStepType
{
    Build,
    Deploy,
    Playwright
}

public sealed class Pipeline
{
    private Pipeline()
    {
    }

    public Pipeline(Guid projectId, string name, string? description, bool isActive = true)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        CreatedAt = DateTimeOffset.UtcNow;
        Update(name, description, isActive);
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public List<PipelineStep> Steps { get; private set; } = [];

    public void RemoveStep(Guid stepId)
    {
        var step = Steps.SingleOrDefault(item => item.Id == stepId);
        if (step is null)
        {
            return;
        }

        Steps.Remove(step);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string name, string? description, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Pipeline name is required.", nameof(name));
        }

        if (name.Length > 120)
        {
            throw new ArgumentException("Pipeline name cannot exceed 120 characters.", nameof(name));
        }

        if (description?.Length > 2000)
        {
            throw new ArgumentException("Pipeline description cannot exceed 2000 characters.", nameof(description));
        }

        Name = name.Trim();
        Description = description?.Trim();
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public PipelineStep AddStep(string name, PipelineStepType type, int order, string? configuration)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Pipeline step name is required.", nameof(name));
        }

        if (order < 1)
        {
            throw new ArgumentException("Pipeline step order must be greater than zero.", nameof(order));
        }

        var step = new PipelineStep(Id, name, type, order, configuration);
        Steps.Add(step);
        UpdatedAt = DateTimeOffset.UtcNow;
        return step;
    }
}

public sealed class PipelineStep
{
    private PipelineStep()
    {
    }

    internal PipelineStep(Guid pipelineId, string name, PipelineStepType type, int order, string? configuration)
    {
        Id = Guid.NewGuid();
        PipelineId = pipelineId;
        Update(name, type, order, configuration);
    }

    public Guid Id { get; private set; }

    public Guid PipelineId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public PipelineStepType Type { get; private set; }

    public int Order { get; private set; }

    public string? Configuration { get; private set; }

    public void Update(string name, PipelineStepType type, int order, string? configuration)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Pipeline step name is required.", nameof(name));
        }

        if (order < 1)
        {
            throw new ArgumentException("Pipeline step order must be greater than zero.", nameof(order));
        }

        Name = name.Trim();
        Type = type;
        Order = order;
        Configuration = configuration?.Trim();
    }
}
