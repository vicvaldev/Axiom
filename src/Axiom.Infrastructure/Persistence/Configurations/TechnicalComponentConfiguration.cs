using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

public class TechnicalComponentConfiguration : IEntityTypeConfiguration<TechnicalComponent>
{
    public void Configure(EntityTypeBuilder<TechnicalComponent> builder)
    {
        builder.ToTable("TechnicalComponents");

        builder.HasKey(c => c.ComponentId);

        builder.Property(c => c.ComponentId)
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnType("varchar(200)");

        builder.Property(c => c.TechnicalName)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnType("varchar(500)");

        builder.Property(c => c.ComponentType)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)")
            .HasConversion<string>();

        builder.Property(c => c.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.Environment)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)")
            .HasConversion<string>();

        builder.Property(c => c.Criticality)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)")
            .HasConversion<string>();

        builder.Property(c => c.CreatedAt)
            .HasColumnType("datetime2");

        builder.Property(c => c.UpdatedAt)
            .HasColumnType("datetime2");

        builder.HasOne(c => c.System)
            .WithMany(s => s.TechnicalComponents)
            .HasForeignKey(c => c.SystemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.TechnicalName)
            .HasDatabaseName("IX_TechnicalComponents_TechnicalName");

        builder.HasIndex(c => c.ComponentType)
            .HasDatabaseName("IX_TechnicalComponents_ComponentType");

        builder.HasIndex(c => c.Environment)
            .HasDatabaseName("IX_TechnicalComponents_Environment");
    }
}
