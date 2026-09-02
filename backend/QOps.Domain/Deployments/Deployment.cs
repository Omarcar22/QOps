namespace QOps.Domain.Deployments;

public enum DeploymentStatus
{
    Pending,
    InProgress,
    Succeeded,
    Failed
}

public sealed class Deployment
{
    private Deployment()
    {
    }

    public Deployment(
        Guid projectId,
        Guid environmentId,
        string version,
        string? notes,
        DeploymentStatus status = DeploymentStatus.Pending)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        EnvironmentId = environmentId;
        CreatedAt = DateTimeOffset.UtcNow;
        Update(version, notes, status);
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public Guid EnvironmentId { get; private set; }

    public string Version { get; private set; } = string.Empty;

    public string? Notes { get; private set; }

    public DeploymentStatus Status { get; private set; }

    public DateTimeOffset? DeployedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(string version, string? notes, DeploymentStatus status)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Deployment version is required.", nameof(version));
        }

        if (version.Length > 40)
        {
            throw new ArgumentException("Deployment version cannot exceed 40 characters.", nameof(version));
        }

        if (notes?.Length > 2000)
        {
            throw new ArgumentException("Deployment notes cannot exceed 2000 characters.", nameof(notes));
        }

        var previousStatus = Status;
        Version = version.Trim();
        Notes = notes?.Trim();
        Status = status;

        if (status == DeploymentStatus.Succeeded && previousStatus != DeploymentStatus.Succeeded)
        {
            DeployedAt = DateTimeOffset.UtcNow;
        }
        else if (status != DeploymentStatus.Succeeded)
        {
            DeployedAt = null;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
