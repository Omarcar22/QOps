using Microsoft.EntityFrameworkCore;
using QOps.Domain.Environments;
using QOps.Domain.Projects;
using DomainEnvironment = QOps.Domain.Environments.Environment;

namespace QOps.Infrastructure.Persistence;

public sealed class QOpsDbContext(DbContextOptions<QOpsDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();

    public DbSet<DomainEnvironment> Environments => Set<DomainEnvironment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(project => project.Id);
            entity.Property(project => project.Name).HasMaxLength(120).IsRequired();
            entity.Property(project => project.Description).HasMaxLength(2000);
            entity.Property(project => project.Environment).HasMaxLength(80).IsRequired();
            entity.Property(project => project.Version).HasMaxLength(40).IsRequired();
            entity.Property(project => project.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<DomainEnvironment>(entity =>
        {
            entity.ToTable("Environments");
            entity.HasKey(environment => environment.Id);
            entity.Property(environment => environment.ProjectId).IsRequired();
            entity.Property(environment => environment.Name).HasMaxLength(120).IsRequired();
            entity.Property(environment => environment.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(environment => environment.Url).HasMaxLength(500).IsRequired();
            entity.Property(environment => environment.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasIndex(environment => new { environment.ProjectId, environment.Name }).IsUnique();
        });
    }
}