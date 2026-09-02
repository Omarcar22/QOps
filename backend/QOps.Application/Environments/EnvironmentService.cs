using QOps.Domain.Environments;
using DomainEnvironment = QOps.Domain.Environments.Environment;

namespace QOps.Application.Environments;

public sealed class EnvironmentService(IEnvironmentRepository repository) : IEnvironmentService
{
    public async Task<EnvironmentResponse> CreateAsync(
        Guid projectId,
        CreateEnvironmentRequest request,
        CancellationToken cancellationToken)
    {
        var environment = new DomainEnvironment(projectId, request.Name, request.Type, request.Url);

        await repository.AddAsync(environment, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(environment);
    }

    public async Task<IReadOnlyCollection<EnvironmentResponse>> GetAllAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var environments = await repository.GetAllAsync(projectId, cancellationToken);
        return environments.Select(Map).ToArray();
    }

    public async Task<EnvironmentResponse?> GetByIdAsync(
        Guid projectId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var environment = await repository.GetByIdAsync(projectId, id, cancellationToken);
        return environment is null ? null : Map(environment);
    }

    public async Task<EnvironmentResponse?> UpdateAsync(
        Guid projectId,
        Guid id,
        UpdateEnvironmentRequest request,
        CancellationToken cancellationToken)
    {
        var environment = await repository.GetByIdAsync(projectId, id, cancellationToken);

        if (environment is null)
        {
            return null;
        }

        environment.Update(request.Name, request.Type, request.Url, request.Status);

        await repository.SaveChangesAsync(cancellationToken);
        return Map(environment);
    }

    public async Task<bool> DeleteAsync(Guid projectId, Guid id, CancellationToken cancellationToken)
    {
        var environment = await repository.GetByIdAsync(projectId, id, cancellationToken);

        if (environment is null)
        {
            return false;
        }

        repository.Remove(environment);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static EnvironmentResponse Map(DomainEnvironment environment) => new(
        environment.Id,
        environment.ProjectId,
        environment.Name,
        environment.Type,
        environment.Url,
        environment.Status,
        environment.CreatedAt,
        environment.UpdatedAt);
}
