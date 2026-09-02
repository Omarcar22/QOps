using Microsoft.EntityFrameworkCore;
using QOps.Domain.Deployments;
using QOps.Domain.Environments;
using QOps.Domain.Projects;
using QOps.Domain.Releases;
using DomainEnvironment = QOps.Domain.Environments.Environment;

namespace QOps.Infrastructure.Persistence;

public sealed class QOpsDbContext(DbContextOptions<QOpsDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();

    public DbSet<DomainEnvironment> Environments => Set<DomainEnvironment>();

    public DbSet<Deployment> Deployments => Set<Deployment>();

    public DbSet<Release> Releases => Set<Release>();

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

        modelBuilder.Entity<Deployment>(entity =>
        {
            entity.ToTable("Deployments");
            entity.HasKey(deployment => deployment.Id);
            entity.Property(deployment => deployment.ProjectId).IsRequired();
            entity.Property(deployment => deployment.EnvironmentId).IsRequired();
            entity.Property(deployment => deployment.Version).HasMaxLength(40).IsRequired();
            entity.Property(deployment => deployment.Notes).HasMaxLength(2000);
            entity.Property(deployment => deployment.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasIndex(deployment => new { deployment.ProjectId, deployment.EnvironmentId, deployment.CreatedAt });
        });

        modelBuilder.Entity<Release>(entity =>
        {
            entity.ToTable("Releases");
            entity.HasKey(release => release.Id);
            entity.Property(release => release.ProjectId).IsRequired();
            entity.Property(release => release.Version).HasMaxLength(40).IsRequired();
            entity.Property(release => release.Notes).HasMaxLength(2000);
            entity.Property(release => release.CommitSha).HasMaxLength(100);
            entity.Property(release => release.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasIndex(release => new { release.ProjectId, release.Version }).IsUnique();
        });
    }
}