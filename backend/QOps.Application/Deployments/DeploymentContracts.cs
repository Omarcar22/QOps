using QOps.Domain.Deployments;
using DomainDeployment = QOps.Domain.Deployments.Deployment;

namespace QOps.Application.Deployments;

public sealed record CreateDeploymentRequest(
    string Version,
    string? Notes);

public sealed record UpdateDeploymentRequest(
    string Version,
    string? Notes,
    DeploymentStatus Status);

public sealed record DeploymentResponse(
    Guid Id,
    Guid ProjectId,
    Guid EnvironmentId,
    string Version,
    string? Notes,
    DeploymentStatus Status,
    DateTimeOffset? DeployedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public interface IDeploymentService
{
    Task<DeploymentResponse> CreateAsync(Guid projectId, Guid environmentId, CreateDeploymentRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DeploymentResponse>> GetAllAsync(Guid projectId, Guid environmentId, CancellationToken cancellationToken);

    Task<DeploymentResponse?> GetByIdAsync(Guid projectId, Guid environmentId, Guid id, CancellationToken cancellationToken);

    Task<DeploymentResponse?> UpdateAsync(Guid projectId, Guid environmentId, Guid id, UpdateDeploymentRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid projectId, Guid environmentId, Guid id, CancellationToken cancellationToken);
}

public interface IDeploymentRepository
{
    Task AddAsync(DomainDeployment deployment, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DomainDeployment>> GetAllAsync(Guid projectId, Guid environmentId, CancellationToken cancellationToken);

    Task<DomainDeployment?> GetByIdAsync(Guid projectId, Guid environmentId, Guid id, CancellationToken cancellationToken);

    void Remove(DomainDeployment deployment);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
