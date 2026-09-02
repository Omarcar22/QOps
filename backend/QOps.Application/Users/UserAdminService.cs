using QOps.Domain.Users;

namespace QOps.Application.Users;

public interface IUserAdminService
{
    Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken);
}

public sealed class UserAdminService(IUserRepository repository) : IUserAdminService
{
    public async Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await repository.GetAllAsync(cancellationToken);
        return users.Select(Map).ToArray();
    }

    public async Task<UserResponse?> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.SetRole(request.Role);
        user.SetActive(request.IsActive);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    private static UserResponse Map(User user) => new(user.Id, user.Email, user.Role, user.IsActive);
}
