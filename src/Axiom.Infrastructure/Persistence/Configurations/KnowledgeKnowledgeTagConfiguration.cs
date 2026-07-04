using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de la entidad <see cref="KnowledgeKnowledgeTag"/> para Entity Framework Core.
/// Define la tabla "KnowledgeKnowledgeTags" como una tabla intermedia (join table) para
/// la relación muchos a muchos entre <see cref="Knowledge"/> y <see cref="KnowledgeTag"/>.
/// Establece una clave primaria compuesta por <c>KnowledgeId</c> y <c>KnowledgeTagId</c>,
/// y configura ambas relaciones con eliminación en cascada.
/// </summary>
public class KnowledgeKnowledgeTagConfiguration : IEntityTypeConfiguration<KnowledgeKnowledgeTag>
{
    /// <summary>
    /// Configura el mapeo de la entidad <see cref="KnowledgeKnowledgeTag"/> en el modelo de EF Core.
    /// </summary>
    /// <param name="builder">Constructor de la configuración de la entidad <see cref="KnowledgeKnowledgeTag"/>.</param>
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
