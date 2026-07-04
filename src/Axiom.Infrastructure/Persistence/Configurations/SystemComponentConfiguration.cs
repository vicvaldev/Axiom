using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

public class SystemComponentConfiguration : IEntityTypeConfiguration<SystemComponent>
{
    public void Configure(EntityTypeBuilder<SystemComponent> builder)
    {
        builder.ToTable("SystemComponents");

        builder.HasKey(sc => sc.SystemComponentId);

        builder.Property(sc => sc.SystemComponentId)
            .ValueGeneratedNever();

        builder.Property(sc => sc.RoleDescription)
            .HasMaxLength(500)
            .HasColumnType("varchar(500)");

        builder.Property(sc => sc.IsOwner)
            .IsRequired()
            .HasColumnType("bit");

        builder.Property(sc => sc.CreatedAt)
            .HasColumnType("datetime2");

        builder.Property(sc => sc.UpdatedAt)
            .HasColumnType("datetime2");

        builder.HasOne(sc => sc.System)
            .WithMany(s => s.SystemComponents)
            .HasForeignKey(sc => sc.SystemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sc => sc.Component)
            .WithMany(tc => tc.SystemComponents)
            .HasForeignKey(sc => sc.ComponentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sc => new { sc.SystemId, sc.ComponentId })
            .IsUnique()
            .HasDatabaseName("IX_SystemComponents_SystemId_ComponentId");

        builder.HasIndex(sc => sc.SystemId)
            .HasDatabaseName("IX_SystemComponents_SystemId");

        builder.HasIndex(sc => sc.ComponentId)
            .HasDatabaseName("IX_SystemComponents_ComponentId");

        builder.HasIndex(sc => sc.IsOwner)
            .HasDatabaseName("IX_SystemComponents_IsOwner");
    }
}
