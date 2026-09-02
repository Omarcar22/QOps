namespace QOps.Domain.Releases;

public enum ReleaseStatus
{
    Draft,
    Published,
    Archived
}

public sealed class Release
{
    private Release()
    {
    }

    public Release(
        Guid projectId,
        string version,
        string? notes,
        string? commitSha,
        ReleaseStatus status = ReleaseStatus.Draft)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        CreatedAt = DateTimeOffset.UtcNow;
        Update(version, notes, commitSha, status);
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Version { get; private set; } = string.Empty;

    public string? Notes { get; private set; }

    public string? CommitSha { get; private set; }

    public ReleaseStatus Status { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(string version, string? notes, string? commitSha, ReleaseStatus status)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Release version is required.", nameof(version));
        }

        if (version.Length > 40)
        {
            throw new ArgumentException("Release version cannot exceed 40 characters.", nameof(version));
        }

        if (notes?.Length > 2000)
        {
            throw new ArgumentException("Release notes cannot exceed 2000 characters.", nameof(notes));
        }

        if (commitSha?.Length > 100)
        {
            throw new ArgumentException("Commit SHA cannot exceed 100 characters.", nameof(commitSha));
        }

        var wasPublished = Status == ReleaseStatus.Published;
        Version = version.Trim();
        Notes = notes?.Trim();
        CommitSha = commitSha?.Trim();
        Status = status;

        if (status == ReleaseStatus.Published && !wasPublished)
        {
            PublishedAt = DateTimeOffset.UtcNow;
        }
        else if (status != ReleaseStatus.Published)
        {
            PublishedAt = null;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
