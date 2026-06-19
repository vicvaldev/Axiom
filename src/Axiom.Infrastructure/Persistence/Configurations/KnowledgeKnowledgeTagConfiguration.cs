using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

public class KnowledgeKnowledgeTagConfiguration : IEntityTypeConfiguration<KnowledgeKnowledgeTag>
{
    public void Configure(EntityTypeBuilder<KnowledgeKnowledgeTag> builder)
    {
        builder.ToTable("KnowledgeKnowledgeTags");

        builder.HasKey(t => new { t.KnowledgeId, t.KnowledgeTagId });

        builder.HasOne(t => t.Knowledge)
            .WithMany(k => k.KnowledgeKnowledgeTags)
            .HasForeignKey(t => t.KnowledgeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Tag)
            .WithMany(t => t.KnowledgeKnowledgeTags)
            .HasForeignKey(t => t.KnowledgeTagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
