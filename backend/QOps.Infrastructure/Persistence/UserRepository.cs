using Microsoft.EntityFrameworkCore;
using QOps.Application.Users;
using QOps.Domain.Users;

namespace QOps.Infrastructure.Persistence;

public sealed class UserRepository(QOpsDbContext dbContext) : IUserRepository
{
    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return dbContext.Users.SingleOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
