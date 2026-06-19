using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

public class AxiomSystemConfiguration : IEntityTypeConfiguration<AxiomSystem>
{
    public void Configure(EntityTypeBuilder<AxiomSystem> builder)
    {
        builder.ToTable("Systems");

        builder.HasKey(s => s.SystemId);

        builder.Property(s => s.SystemId)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.EAI)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnType("varchar(20)");

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnType("varchar(200)");

        builder.HasOne(s => s.Owner)
            .WithMany(u => u.OwnedSystems)
            .HasForeignKey(s => s.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.OwnerUserId)
            .HasDatabaseName("IX_Systems_OwnerUserId");
    }
}
