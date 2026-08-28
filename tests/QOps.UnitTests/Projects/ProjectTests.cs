using QOps.Application.Projects;
using QOps.Domain.Projects;

namespace QOps.UnitTests.Projects;

public class ProjectTests
{
    [Fact]
    public void Create_WithBlankName_ShouldThrow()
    {
        var action = () => new Project(" ", null, "Test", "1.0.0");

        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public async Task Service_Create_ShouldReturnPersistedProject()
    {
        var repository = new InMemoryProjectRepository();
        var service = new ProjectService(repository);

        var result = await service.CreateAsync(
            new CreateProjectRequest("QOps", "Quality platform", "Test", "1.0.0"),
            CancellationToken.None);

        Assert.Equal("QOps", result.Name);
        Assert.Equal(ProjectStatus.Active, result.Status);
        Assert.Contains(repository.Projects, project => project.Id == result.Id);
    }

    private sealed class InMemoryProjectRepository : IProjectRepository
    {
        public List<Project> Projects { get; } = [];

        public Task AddAsync(Project project, CancellationToken cancellationToken)
        {
            Projects.Add(project);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Project>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<Project>>(Projects);
        }

        public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Projects.SingleOrDefault(project => project.Id == id));
        }

        public void Remove(Project project)
        {
            Projects.Remove(project);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}