using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

public class KnowledgeTypeConfiguration : IEntityTypeConfiguration<KnowledgeType>
{
    public void Configure(EntityTypeBuilder<KnowledgeType> builder)
    {
        builder.ToTable("KnowledgeTypes");

        builder.HasKey(t => t.TypeId);

        builder.Property(t => t.TypeId)
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Code)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnType("varchar(200)");

        builder.HasIndex(t => t.Code)
            .IsUnique()
            .HasDatabaseName("IX_KnowledgeTypes_Code");
    }
}
