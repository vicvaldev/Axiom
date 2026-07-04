using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de la entidad <see cref="AxiomSystem"/> para Entity Framework Core.
/// Define la tabla "Systems", su clave primaria autoincremental, propiedades obligatorias
/// con restricciones de longitud y tipo, la relación con <see cref="User"/> como propietario,
/// e índices sobre la clave foránea del propietario.
/// </summary>
public class AxiomSystemConfiguration : IEntityTypeConfiguration<AxiomSystem>
{
    /// <summary>
    /// Configura el mapeo de la entidad <see cref="AxiomSystem"/> en el modelo de EF Core.
    /// </summary>
    /// <param name="builder">Constructor de la configuración de la entidad <see cref="AxiomSystem"/>.</param>
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
