using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de la entidad <see cref="Knowledge"/> para Entity Framework Core.
/// Define la tabla "Knowledges", su clave primaria (GUID generado por la aplicación),
/// propiedades obligatorias y opcionales con restricciones de longitud y tipo,
/// las relaciones con <see cref="AxiomSystem"/>, <see cref="User"/>, <see cref="KnowledgeType"/>,
/// <see cref="KnowledgeState"/> e <see cref="Issue"/>, e índices sobre las claves foráneas.
/// </summary>
public class KnowledgeConfiguration : IEntityTypeConfiguration<Knowledge>
{
    /// <summary>
    /// Configura el mapeo de la entidad <see cref="Knowledge"/> en el modelo de EF Core.
    /// </summary>
    /// <param name="builder">Constructor de la configuración de la entidad <see cref="Knowledge"/>.</param>
    public void Configure(EntityTypeBuilder<Knowledge> builder)
    {
        builder.ToTable("Knowledges");

        builder.HasKey(k => k.KnowledgeId);

        builder.Property(k => k.KnowledgeId)
            .ValueGeneratedNever();

        builder.Property(k => k.Title)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnType("varchar(500)");

        builder.Property(k => k.Summary)
            .HasMaxLength(200)
            .HasColumnType("varchar(200)");

        builder.Property(k => k.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(k => k.CreatedAt)
            .HasColumnType("datetime2");

        builder.Property(k => k.UpdatedAt)
            .HasColumnType("datetime2");

        builder.Property(k => k.VersionNumber)
            .IsRequired()
            .HasColumnType("int");

        builder.HasOne(k => k.System)
            .WithMany(s => s.Knowledges)
            .HasForeignKey(k => k.SystemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(k => k.CreatedBy)
            .WithMany(u => u.CreatedKnowledges)
            .HasForeignKey(k => k.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(k => k.Type)
            .WithMany(t => t.Knowledges)
            .HasForeignKey(k => k.KnowledgeTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(k => k.State)
            .WithMany(s => s.Knowledges)
            .HasForeignKey(k => k.KnowledgeStateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(k => k.Issue)
            .WithMany(i => i.Knowledges)
            .HasForeignKey(k => k.IssueId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(k => k.SystemId)
            .HasDatabaseName("IX_Knowledges_SystemId");

        builder.HasIndex(k => k.CreatedByUserId)
            .HasDatabaseName("IX_Knowledges_CreatedByUserId");

        builder.HasIndex(k => k.KnowledgeTypeId)
            .HasDatabaseName("IX_Knowledges_KnowledgeTypeId");

        builder.HasIndex(k => k.KnowledgeStateId)
            .HasDatabaseName("IX_Knowledges_KnowledgeStateId");

        builder.HasIndex(k => k.IssueId)
            .HasDatabaseName("IX_Knowledges_IssueId");
    }
}
