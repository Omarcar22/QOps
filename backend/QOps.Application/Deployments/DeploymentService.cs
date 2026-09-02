using QOps.Domain.Deployments;
using DomainDeployment = QOps.Domain.Deployments.Deployment;

namespace QOps.Application.Deployments;

public sealed class DeploymentService(IDeploymentRepository repository) : IDeploymentService
{
    public async Task<DeploymentResponse> CreateAsync(
        Guid projectId,
        Guid environmentId,
        CreateDeploymentRequest request,
        CancellationToken cancellationToken)
    {
        var deployment = new DomainDeployment(projectId, environmentId, request.Version, request.Notes);

        await repository.AddAsync(deployment, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(deployment);
    }

    public async Task<IReadOnlyCollection<DeploymentResponse>> GetAllAsync(
        Guid projectId,
        Guid environmentId,
        CancellationToken cancellationToken)
    {
        var deployments = await repository.GetAllAsync(projectId, environmentId, cancellationToken);
        return deployments.Select(Map).ToArray();
    }

    public async Task<DeploymentResponse?> GetByIdAsync(
        Guid projectId,
        Guid environmentId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var deployment = await repository.GetByIdAsync(projectId, environmentId, id, cancellationToken);
        return deployment is null ? null : Map(deployment);
    }

    public async Task<DeploymentResponse?> UpdateAsync(
        Guid projectId,
        Guid environmentId,
        Guid id,
        UpdateDeploymentRequest request,
        CancellationToken cancellationToken)
    {
        var deployment = await repository.GetByIdAsync(projectId, environmentId, id, cancellationToken);

        if (deployment is null)
        {
            return null;
        }

        deployment.Update(request.Version, request.Notes, request.Status);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(deployment);
    }

    public async Task<bool> DeleteAsync(
        Guid projectId,
        Guid environmentId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var deployment = await repository.GetByIdAsync(projectId, environmentId, id, cancellationToken);

        if (deployment is null)
        {
            return false;
        }

        repository.Remove(deployment);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static DeploymentResponse Map(DomainDeployment deployment) => new(
        deployment.Id,
        deployment.ProjectId,
        deployment.EnvironmentId,
        deployment.Version,
        deployment.Notes,
        deployment.Status,
        deployment.DeployedAt,
        deployment.CreatedAt,
        deployment.UpdatedAt);
}
