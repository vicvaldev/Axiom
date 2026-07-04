using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de la entidad <see cref="Issue"/> para Entity Framework Core.
/// Define la tabla "Issues", su clave primaria (GUID generado por la aplicación),
/// propiedades obligatorias y opcionales con restricciones de longitud y tipo,
/// las relaciones con <see cref="AxiomSystem"/>, <see cref="IssueState"/> y <see cref="User"/>,
/// e índices únicos filtrados para <c>RitmNumber</c> e <c>IncidentNumber</c>.
/// </summary>
public class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    /// <summary>
    /// Configura el mapeo de la entidad <see cref="Issue"/> en el modelo de EF Core.
    /// </summary>
    /// <param name="builder">Constructor de la configuración de la entidad <see cref="Issue"/>.</param>
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.ToTable("Issues");

        builder.HasKey(i => i.IssueId);

        builder.Property(i => i.IssueId)
            .ValueGeneratedNever();

        builder.Property(i => i.Summary)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnType("varchar(200)");

        builder.Property(i => i.RitmNumber)
            .HasMaxLength(20)
            .HasColumnType("varchar(20)");

        builder.Property(i => i.IncidentNumber)
            .HasMaxLength(20)
            .HasColumnType("varchar(20)");

        builder.Property(i => i.Problem)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.Analysis)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.Resolution)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.CreatedAt)
            .HasColumnType("datetime2");

        builder.Property(i => i.UpdatedAt)
            .HasColumnType("datetime2");

        builder.Property(i => i.ResolvedAt)
            .HasColumnType("datetime2");

        builder.HasOne(i => i.System)
            .WithMany(s => s.Issues)
            .HasForeignKey(i => i.SystemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.State)
            .WithMany(s => s.Issues)
            .HasForeignKey(i => i.StateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.CreatedBy)
            .WithMany(u => u.CreatedIssues)
            .HasForeignKey(i => i.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.RitmNumber)
            .IsUnique()
            .HasDatabaseName("IX_Issues_RitmNumber")
            .HasFilter("[RitmNumber] IS NOT NULL");

        builder.HasIndex(i => i.IncidentNumber)
            .IsUnique()
            .HasDatabaseName("IX_Issues_IncidentNumber")
            .HasFilter("[IncidentNumber] IS NOT NULL");

        builder.HasIndex(i => i.SystemId)
            .HasDatabaseName("IX_Issues_SystemId");

        builder.HasIndex(i => i.StateId)
            .HasDatabaseName("IX_Issues_StateId");

        builder.HasIndex(i => i.CreatedByUserId)
            .HasDatabaseName("IX_Issues_CreatedByUserId");
    }
}
