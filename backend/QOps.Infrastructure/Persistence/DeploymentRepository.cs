using Microsoft.EntityFrameworkCore;
using QOps.Application.Deployments;
using DomainDeployment = QOps.Domain.Deployments.Deployment;

namespace QOps.Infrastructure.Persistence;

public sealed class DeploymentRepository(QOpsDbContext dbContext) : IDeploymentRepository
{
    public async Task AddAsync(DomainDeployment deployment, CancellationToken cancellationToken)
    {
        await dbContext.Deployments.AddAsync(deployment, cancellationToken);
    }

    public async Task<IReadOnlyCollection<DomainDeployment>> GetAllAsync(
        Guid projectId,
        Guid environmentId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Deployments
            .AsNoTracking()
            .Where(deployment => deployment.ProjectId == projectId && deployment.EnvironmentId == environmentId)
            .OrderByDescending(deployment => deployment.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task<DomainDeployment?> GetByIdAsync(
        Guid projectId,
        Guid environmentId,
        Guid id,
        CancellationToken cancellationToken)
    {
        return dbContext.Deployments.SingleOrDefaultAsync(
            deployment => deployment.ProjectId == projectId
                && deployment.EnvironmentId == environmentId
                && deployment.Id == id,
            cancellationToken);
    }

    public void Remove(DomainDeployment deployment)
    {
        dbContext.Deployments.Remove(deployment);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
