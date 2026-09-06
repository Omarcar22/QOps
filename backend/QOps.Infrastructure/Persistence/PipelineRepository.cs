using Microsoft.EntityFrameworkCore;
using QOps.Application.Pipelines;
using QOps.Domain.Pipelines;

namespace QOps.Infrastructure.Persistence;

public sealed class PipelineRepository(QOpsDbContext dbContext) : IPipelineRepository
{
    public async Task AddAsync(Pipeline pipeline, CancellationToken cancellationToken)
    {
        await dbContext.Pipelines.AddAsync(pipeline, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Pipeline>> GetAllAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return await dbContext.Pipelines
            .AsNoTracking()
            .Include(pipeline => pipeline.Steps)
            .Where(pipeline => pipeline.ProjectId == projectId)
            .OrderBy(pipeline => pipeline.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Pipeline?> GetByIdAsync(Guid projectId, Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Pipelines
            .Include(pipeline => pipeline.Steps)
            .SingleOrDefaultAsync(pipeline => pipeline.ProjectId == projectId && pipeline.Id == id, cancellationToken);
    }

    public async Task AddExecutionAsync(PipelineExecution execution, CancellationToken cancellationToken)
    {
        await dbContext.PipelineExecutions.AddAsync(execution, cancellationToken);
    }

    public async Task<IReadOnlyCollection<PipelineExecution>> GetExecutionsAsync(Guid projectId, Guid pipelineId, CancellationToken cancellationToken)
    {
        return await dbContext.PipelineExecutions
            .AsNoTracking()
            .Where(execution => execution.ProjectId == projectId && execution.PipelineId == pipelineId)
            .OrderByDescending(execution => execution.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public void Remove(Pipeline pipeline)
    {
        dbContext.Pipelines.Remove(pipeline);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
