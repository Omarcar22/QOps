using QOps.Domain.Environments;
using DomainEnvironment = QOps.Domain.Environments.Environment;

namespace QOps.Application.Environments;

public sealed record CreateEnvironmentRequest(
    string Name,
    EnvironmentType Type,
    string Url);

public sealed record UpdateEnvironmentRequest(
    string Name,
    EnvironmentType Type,
    string Url,
    EnvironmentStatus Status);

public sealed record EnvironmentResponse(
    Guid Id,
    Guid ProjectId,
    string Name,
    EnvironmentType Type,
    string Url,
    EnvironmentStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public interface IEnvironmentService
{
    Task<EnvironmentResponse> CreateAsync(Guid projectId, CreateEnvironmentRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<EnvironmentResponse>> GetAllAsync(Guid projectId, CancellationToken cancellationToken);

    Task<EnvironmentResponse?> GetByIdAsync(Guid projectId, Guid id, CancellationToken cancellationToken);

    Task<EnvironmentResponse?> UpdateAsync(Guid projectId, Guid id, UpdateEnvironmentRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid projectId, Guid id, CancellationToken cancellationToken);
}

public interface IEnvironmentRepository
{
    Task AddAsync(DomainEnvironment environment, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DomainEnvironment>> GetAllAsync(Guid projectId, CancellationToken cancellationToken);

    Task<DomainEnvironment?> GetByIdAsync(Guid projectId, Guid id, CancellationToken cancellationToken);

    void Remove(DomainEnvironment environment);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
