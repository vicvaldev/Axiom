using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de la entidad <see cref="IssueState"/> para Entity Framework Core.
/// Define la tabla "IssueStates", su clave primaria autoincremental, las propiedades
/// obligatorias <c>Code</c> y <c>Name</c> con restricciones de longitud, y un índice único sobre el código.
/// </summary>
public class IssueStateConfiguration : IEntityTypeConfiguration<IssueState>
{
    /// <summary>
    /// Configura el mapeo de la entidad <see cref="IssueState"/> en el modelo de EF Core.
    /// </summary>
    /// <param name="builder">Constructor de la configuración de la entidad <see cref="IssueState"/>.</param>
    public void Configure(EntityTypeBuilder<IssueState> builder)
    {
        builder.ToTable("IssueStates");

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
            .HasDatabaseName("IX_IssueStates_Code");
    }
}
