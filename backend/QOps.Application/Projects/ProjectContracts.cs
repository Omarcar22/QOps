using QOps.Domain.Projects;

namespace QOps.Application.Projects;

public sealed record CreateProjectRequest(
    string Name,
    string? Description,
    string Environment,
    string Version);

public sealed record UpdateProjectRequest(
    string Name,
    string? Description,
    string Environment,
    string Version,
    ProjectStatus Status);

public sealed record ProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    string Environment,
    string Version,
    ProjectStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public interface IProjectService
{
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ProjectResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProjectResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ProjectResponse?> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public interface IProjectRepository
{
    Task AddAsync(Project project, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Project>> GetAllAsync(CancellationToken cancellationToken);

    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Remove(Project project);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}