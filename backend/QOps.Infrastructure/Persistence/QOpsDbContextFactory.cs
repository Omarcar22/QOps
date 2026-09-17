using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QOps.Infrastructure.Persistence;

public sealed class QOpsDbContextFactory : IDesignTimeDbContextFactory<QOpsDbContext>
{
    public QOpsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("QOPS_DATABASE_CONNECTION");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("QOPS_DATABASE_CONNECTION must be configured for design-time migrations.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<QOpsDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new QOpsDbContext(optionsBuilder.Options);
    }
}