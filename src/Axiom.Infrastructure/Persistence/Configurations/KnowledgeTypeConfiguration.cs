using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de la entidad <see cref="KnowledgeType"/> para Entity Framework Core.
/// Define la tabla "KnowledgeTypes", su clave primaria autoincremental, las propiedades
/// obligatorias <c>Code</c> y <c>Name</c> con restricciones de longitud, y un índice único sobre el código.
/// </summary>
public class KnowledgeTypeConfiguration : IEntityTypeConfiguration<KnowledgeType>
{
    /// <summary>
    /// Configura el mapeo de la entidad <see cref="KnowledgeType"/> en el modelo de EF Core.
    /// </summary>
    /// <param name="builder">Constructor de la configuración de la entidad <see cref="KnowledgeType"/>.</param>
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
