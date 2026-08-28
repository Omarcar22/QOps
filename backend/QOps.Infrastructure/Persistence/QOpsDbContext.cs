using Microsoft.EntityFrameworkCore;
using QOps.Domain.Projects;

namespace QOps.Infrastructure.Persistence;

public sealed class QOpsDbContext(DbContextOptions<QOpsDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();

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
    }
}