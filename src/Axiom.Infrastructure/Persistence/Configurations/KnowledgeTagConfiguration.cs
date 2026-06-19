using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

public class KnowledgeTagConfiguration : IEntityTypeConfiguration<KnowledgeTag>
{
    public void Configure(EntityTypeBuilder<KnowledgeTag> builder)
    {
        builder.ToTable("KnowledgeTags");

        builder.HasKey(t => t.KnowledgeTagId);

        builder.Property(t => t.KnowledgeTagId)
            .ValueGeneratedOnAdd();

        builder.Property(t => t.TagName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.HasIndex(t => t.TagName)
            .IsUnique()
            .HasDatabaseName("IX_KnowledgeTags_TagName");
    }
}
