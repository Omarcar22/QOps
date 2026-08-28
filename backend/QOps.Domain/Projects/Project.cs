namespace QOps.Domain.Projects;

public sealed class Project
{
    private Project()
    {
    }

    public Project(
        string name,
        string? description,
        string environment,
        string version,
        ProjectStatus status = ProjectStatus.Active)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
        Update(name, description, environment, version, status);
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string Environment { get; private set; } = string.Empty;

    public string Version { get; private set; } = string.Empty;

    public ProjectStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        string name,
        string? description,
        string environment,
        string version,
        ProjectStatus status)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Project name is required.", nameof(name));
        }

        if (name.Length > 120)
        {
            throw new ArgumentException("Project name cannot exceed 120 characters.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(environment))
        {
            throw new ArgumentException("Project environment is required.", nameof(environment));
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Project version is required.", nameof(version));
        }

        Name = name.Trim();
        Description = description?.Trim();
        Environment = environment.Trim();
        Version = version.Trim();
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}