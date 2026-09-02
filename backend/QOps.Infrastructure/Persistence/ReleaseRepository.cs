using Microsoft.EntityFrameworkCore;
using QOps.Application.Releases;
using DomainRelease = QOps.Domain.Releases.Release;

namespace QOps.Infrastructure.Persistence;

public sealed class ReleaseRepository(QOpsDbContext dbContext) : IReleaseRepository
{
    public async Task AddAsync(DomainRelease release, CancellationToken cancellationToken)
    {
        await dbContext.Releases.AddAsync(release, cancellationToken);
    }

    public async Task<IReadOnlyCollection<DomainRelease>> GetAllAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return await dbContext.Releases
            .AsNoTracking()
            .Where(release => release.ProjectId == projectId)
            .OrderByDescending(release => release.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task<DomainRelease?> GetByIdAsync(Guid projectId, Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Releases.SingleOrDefaultAsync(
            release => release.ProjectId == projectId && release.Id == id,
            cancellationToken);
    }

    public void Remove(DomainRelease release)
    {
        dbContext.Releases.Remove(release);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
