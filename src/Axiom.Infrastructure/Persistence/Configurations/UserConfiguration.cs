using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de la entidad <see cref="User"/> para Entity Framework Core.
/// Define la tabla "Users", su clave primaria, propiedades obligatorias con restricciones
/// de longitud y tipo, e índices únicos sobre el correo electrónico.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Configura el mapeo de la entidad <see cref="User"/> en el modelo de EF Core.
    /// </summary>
    /// <param name="builder">Constructor de la configuración de la entidad <see cref="User"/>.</param>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.UserId);

        builder.Property(u => u.UserId)
            .ValueGeneratedNever();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnType("varchar(200)");

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnType("varchar(200)");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");
    }
}
