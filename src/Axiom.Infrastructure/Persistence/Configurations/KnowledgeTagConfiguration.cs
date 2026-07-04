using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de la entidad <see cref="KnowledgeTag"/> para Entity Framework Core.
/// Define la tabla "KnowledgeTags", su clave primaria autoincremental, la propiedad
/// obligatoria <c>TagName</c> con restricción de longitud, y un índice único sobre el nombre de la etiqueta.
/// </summary>
public class KnowledgeTagConfiguration : IEntityTypeConfiguration<KnowledgeTag>
{
    /// <summary>
    /// Configura el mapeo de la entidad <see cref="KnowledgeTag"/> en el modelo de EF Core.
    /// </summary>
    /// <param name="builder">Constructor de la configuración de la entidad <see cref="KnowledgeTag"/>.</param>
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
