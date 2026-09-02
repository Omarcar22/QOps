using QOps.Domain.Releases;
using DomainRelease = QOps.Domain.Releases.Release;

namespace QOps.Application.Releases;

public sealed record CreateReleaseRequest(
    string Version,
    string? Notes,
    string? CommitSha);

public sealed record UpdateReleaseRequest(
    string Version,
    string? Notes,
    string? CommitSha,
    ReleaseStatus Status);

public sealed record ReleaseResponse(
    Guid Id,
    Guid ProjectId,
    string Version,
    string? Notes,
    string? CommitSha,
    ReleaseStatus Status,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public interface IReleaseService
{
    Task<ReleaseResponse> CreateAsync(Guid projectId, CreateReleaseRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReleaseResponse>> GetAllAsync(Guid projectId, CancellationToken cancellationToken);

    Task<ReleaseResponse?> GetByIdAsync(Guid projectId, Guid id, CancellationToken cancellationToken);

    Task<ReleaseResponse?> UpdateAsync(Guid projectId, Guid id, UpdateReleaseRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid projectId, Guid id, CancellationToken cancellationToken);
}

public interface IReleaseRepository
{
    Task AddAsync(DomainRelease release, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DomainRelease>> GetAllAsync(Guid projectId, CancellationToken cancellationToken);

    Task<DomainRelease?> GetByIdAsync(Guid projectId, Guid id, CancellationToken cancellationToken);

    void Remove(DomainRelease release);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
