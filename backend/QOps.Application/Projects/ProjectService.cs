using QOps.Domain.Projects;

namespace QOps.Application.Projects;

public sealed class ProjectService(IProjectRepository repository) : IProjectService
{
    public async Task<ProjectResponse> CreateAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var project = new Project(
            request.Name,
            request.Description,
            request.Environment,
            request.Version);

        await repository.AddAsync(project, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(project);
    }

    public async Task<IReadOnlyCollection<ProjectResponse>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var projects = await repository.GetAllAsync(cancellationToken);
        return projects.Select(Map).ToArray();
    }

    public async Task<ProjectResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var project = await repository.GetByIdAsync(id, cancellationToken);
        return project is null ? null : Map(project);
    }

    public async Task<ProjectResponse?> UpdateAsync(
        Guid id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var project = await repository.GetByIdAsync(id, cancellationToken);

        if (project is null)
        {
            return null;
        }

        project.Update(
            request.Name,
            request.Description,
            request.Environment,
            request.Version,
            request.Status);

        await repository.SaveChangesAsync(cancellationToken);
        return Map(project);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var project = await repository.GetByIdAsync(id, cancellationToken);

        if (project is null)
        {
            return false;
        }

        repository.Remove(project);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ProjectResponse Map(Project project) => new(
        project.Id,
        project.Name,
        project.Description,
        project.Environment,
        project.Version,
        project.Status,
        project.CreatedAt,
        project.UpdatedAt);
}