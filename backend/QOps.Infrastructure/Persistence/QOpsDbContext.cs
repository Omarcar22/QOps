using Microsoft.EntityFrameworkCore;
using QOps.Domain.Deployments;
using QOps.Domain.Environments;
using QOps.Domain.Pipelines;
using QOps.Domain.Projects;
using QOps.Domain.Releases;
using QOps.Domain.Users;
using DomainEnvironment = QOps.Domain.Environments.Environment;

namespace QOps.Infrastructure.Persistence;

public sealed class QOpsDbContext(DbContextOptions<QOpsDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();

    public DbSet<DomainEnvironment> Environments => Set<DomainEnvironment>();

    public DbSet<Deployment> Deployments => Set<Deployment>();

    public DbSet<Release> Releases => Set<Release>();

    public DbSet<Pipeline> Pipelines => Set<Pipeline>();

    public DbSet<PipelineStep> PipelineSteps => Set<PipelineStep>();

    public DbSet<PipelineExecution> PipelineExecutions => Set<PipelineExecution>();

    public DbSet<User> Users => Set<User>();

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

        modelBuilder.Entity<Pipeline>(entity =>
        {
            entity.ToTable("Pipelines");
            entity.HasKey(pipeline => pipeline.Id);
            entity.Property(pipeline => pipeline.ProjectId).IsRequired();
            entity.Property(pipeline => pipeline.Name).HasMaxLength(120).IsRequired();
            entity.Property(pipeline => pipeline.Description).HasMaxLength(2000);
            entity.Property(pipeline => pipeline.IsActive).IsRequired();
            entity.HasIndex(pipeline => new { pipeline.ProjectId, pipeline.Name }).IsUnique();
            entity.HasMany(pipeline => pipeline.Steps)
                .WithOne()
                .HasForeignKey(step => step.PipelineId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PipelineStep>(entity =>
        {
            entity.ToTable("PipelineSteps");
            entity.HasKey(step => step.Id);
            entity.Property(step => step.PipelineId).IsRequired();
            entity.Property(step => step.Name).HasMaxLength(120).IsRequired();
            entity.Property(step => step.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(step => step.Order).IsRequired();
            entity.Property(step => step.Configuration).HasMaxLength(2000);
            entity.HasIndex(step => new { step.PipelineId, step.Order }).IsUnique();
        });

        modelBuilder.Entity<PipelineExecution>(entity =>
        {
            entity.ToTable("PipelineExecutions");
            entity.HasKey(execution => execution.Id);
            entity.Property(execution => execution.ProjectId).IsRequired();
            entity.Property(execution => execution.PipelineId).IsRequired();
            entity.Property(execution => execution.TriggeredBy).HasMaxLength(120).IsRequired();
            entity.Property(execution => execution.Notes).HasMaxLength(2000);
            entity.Property(execution => execution.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasIndex(execution => new { execution.ProjectId, execution.PipelineId, execution.CreatedAt });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(user => user.IsActive).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique();
        });
    }
}