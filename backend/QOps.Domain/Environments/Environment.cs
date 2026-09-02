namespace QOps.Domain.Environments;

public enum EnvironmentType
{
    Development,
    Staging,
    Production
}

public enum EnvironmentStatus
{
    Active,
    Inactive
}

public sealed class Environment
{
    private Environment()
    {
    }

    public Environment(
        Guid projectId,
        string name,
        EnvironmentType type,
        string url,
        EnvironmentStatus status = EnvironmentStatus.Active)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        CreatedAt = DateTimeOffset.UtcNow;
        Update(name, type, url, status);
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public EnvironmentType Type { get; private set; }

    public string Url { get; private set; } = string.Empty;

    public EnvironmentStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(string name, EnvironmentType type, string url, EnvironmentStatus status)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Environment name is required.", nameof(name));
        }

        if (name.Length > 120)
        {
            throw new ArgumentException("Environment name cannot exceed 120 characters.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Environment URL is required.", nameof(url));
        }

        Name = name.Trim();
        Type = type;
        Url = url.Trim();
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
