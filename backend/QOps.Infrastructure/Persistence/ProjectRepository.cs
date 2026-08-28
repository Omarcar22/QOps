using Microsoft.EntityFrameworkCore;
using QOps.Application.Projects;
using QOps.Domain.Projects;

namespace QOps.Infrastructure.Persistence;

public sealed class ProjectRepository(QOpsDbContext dbContext) : IProjectRepository
{
    public async Task AddAsync(Project project, CancellationToken cancellationToken)
    {
        await dbContext.Projects.AddAsync(project, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Project>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Projects
            .AsNoTracking()
            .OrderBy(project => project.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Projects.SingleOrDefaultAsync(project => project.Id == id, cancellationToken);
    }

    public void Remove(Project project)
    {
        dbContext.Projects.Remove(project);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}