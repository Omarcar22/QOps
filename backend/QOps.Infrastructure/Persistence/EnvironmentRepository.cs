using Microsoft.EntityFrameworkCore;
using QOps.Application.Environments;
using DomainEnvironment = QOps.Domain.Environments.Environment;

namespace QOps.Infrastructure.Persistence;

public sealed class EnvironmentRepository(QOpsDbContext dbContext) : IEnvironmentRepository
{
    public async Task AddAsync(DomainEnvironment environment, CancellationToken cancellationToken)
    {
        await dbContext.Environments.AddAsync(environment, cancellationToken);
    }

    public async Task<IReadOnlyCollection<DomainEnvironment>> GetAllAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return await dbContext.Environments
            .AsNoTracking()
            .Where(environment => environment.ProjectId == projectId)
            .OrderBy(environment => environment.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<DomainEnvironment?> GetByIdAsync(Guid projectId, Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Environments.SingleOrDefaultAsync(
            environment => environment.ProjectId == projectId && environment.Id == id,
            cancellationToken);
    }

    public void Remove(DomainEnvironment environment)
    {
        dbContext.Environments.Remove(environment);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
