using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de la entidad <see cref="KnowledgeState"/> para Entity Framework Core.
/// Define la tabla "KnowledgeStates", su clave primaria autoincremental, las propiedades
/// obligatorias <c>Code</c> y <c>Name</c> con restricciones de longitud, y un índice único sobre el código.
/// </summary>
public class KnowledgeStateConfiguration : IEntityTypeConfiguration<KnowledgeState>
{
    /// <summary>
    /// Configura el mapeo de la entidad <see cref="KnowledgeState"/> en el modelo de EF Core.
    /// </summary>
    /// <param name="builder">Constructor de la configuración de la entidad <see cref="KnowledgeState"/>.</param>
    public void Configure(EntityTypeBuilder<KnowledgeState> builder)
    {
        builder.ToTable("KnowledgeStates");

        builder.HasKey(s => s.StateId);

        builder.Property(s => s.StateId)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnType("varchar(200)");

        builder.HasIndex(s => s.Code)
            .IsUnique()
            .HasDatabaseName("IX_KnowledgeStates_Code");
    }
}
