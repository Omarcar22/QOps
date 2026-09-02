using QOps.Domain.Releases;
using DomainRelease = QOps.Domain.Releases.Release;

namespace QOps.Application.Releases;

public sealed class ReleaseService(IReleaseRepository repository) : IReleaseService
{
    public async Task<ReleaseResponse> CreateAsync(
        Guid projectId,
        CreateReleaseRequest request,
        CancellationToken cancellationToken)
    {
        var release = new DomainRelease(projectId, request.Version, request.Notes, request.CommitSha);
        await repository.AddAsync(release, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(release);
    }

    public async Task<IReadOnlyCollection<ReleaseResponse>> GetAllAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var releases = await repository.GetAllAsync(projectId, cancellationToken);
        return releases.Select(Map).ToArray();
    }

    public async Task<ReleaseResponse?> GetByIdAsync(
        Guid projectId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var release = await repository.GetByIdAsync(projectId, id, cancellationToken);
        return release is null ? null : Map(release);
    }

    public async Task<ReleaseResponse?> UpdateAsync(
        Guid projectId,
        Guid id,
        UpdateReleaseRequest request,
        CancellationToken cancellationToken)
    {
        var release = await repository.GetByIdAsync(projectId, id, cancellationToken);
        if (release is null)
        {
            return null;
        }

        release.Update(request.Version, request.Notes, request.CommitSha, request.Status);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(release);
    }

    public async Task<bool> DeleteAsync(Guid projectId, Guid id, CancellationToken cancellationToken)
    {
        var release = await repository.GetByIdAsync(projectId, id, cancellationToken);
        if (release is null)
        {
            return false;
        }

        repository.Remove(release);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ReleaseResponse Map(DomainRelease release) => new(
        release.Id,
        release.ProjectId,
        release.Version,
        release.Notes,
        release.CommitSha,
        release.Status,
        release.PublishedAt,
        release.CreatedAt,
        release.UpdatedAt);
}
