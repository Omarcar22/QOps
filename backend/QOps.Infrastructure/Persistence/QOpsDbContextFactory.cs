using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QOps.Infrastructure.Persistence;

public sealed class QOpsDbContextFactory : IDesignTimeDbContextFactory<QOpsDbContext>
{
    public QOpsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("QOPS_DATABASE_CONNECTION")
            ?? "Server=localhost,1433;Database=QOps;User Id=sa;Password=QOps_dev_2026!;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<QOpsDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new QOpsDbContext(optionsBuilder.Options);
    }
}