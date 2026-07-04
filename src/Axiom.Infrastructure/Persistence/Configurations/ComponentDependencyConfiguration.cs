using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

public class ComponentDependencyConfiguration : IEntityTypeConfiguration<ComponentDependency>
{
    public void Configure(EntityTypeBuilder<ComponentDependency> builder)
    {
        builder.ToTable("ComponentDependencies");

        builder.HasKey(d => d.DependencyId);

        builder.Property(d => d.DependencyId)
            .ValueGeneratedNever();

        builder.Property(d => d.DependencyType)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)")
            .HasConversion<string>();

        builder.Property(d => d.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(d => d.Criticality)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)")
            .HasConversion<string>();

        builder.Property(d => d.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)")
            .HasConversion<string>();

        builder.Property(d => d.CreatedAt)
            .HasColumnType("datetime2");

        builder.Property(d => d.UpdatedAt)
            .HasColumnType("datetime2");

        builder.HasOne(d => d.Source)
            .WithMany(tc => tc.OutgoingDependencies)
            .HasForeignKey(d => d.SourceComponentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Target)
            .WithMany(tc => tc.IncomingDependencies)
            .HasForeignKey(d => d.TargetComponentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => new { d.SourceComponentId, d.TargetComponentId, d.DependencyType })
            .IsUnique()
            .HasDatabaseName("IX_ComponentDependencies_Source_Target_Type");

        builder.HasIndex(d => d.SourceComponentId)
            .HasDatabaseName("IX_ComponentDependencies_SourceComponentId");

        builder.HasIndex(d => d.TargetComponentId)
            .HasDatabaseName("IX_ComponentDependencies_TargetComponentId");

        builder.HasIndex(d => d.DependencyType)
            .HasDatabaseName("IX_ComponentDependencies_DependencyType");

        builder.HasIndex(d => d.Status)
            .HasDatabaseName("IX_ComponentDependencies_Status");
    }
}
