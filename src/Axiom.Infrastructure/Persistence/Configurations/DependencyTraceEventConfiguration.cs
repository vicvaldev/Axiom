using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

public class DependencyTraceEventConfiguration : IEntityTypeConfiguration<DependencyTraceEvent>
{
    public void Configure(EntityTypeBuilder<DependencyTraceEvent> builder)
    {
        builder.ToTable("DependencyTraceEvents");

        builder.HasKey(e => e.TraceEventId);

        builder.Property(e => e.TraceEventId)
            .ValueGeneratedNever();

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)")
            .HasConversion<string>();

        builder.Property(e => e.Description)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.RelatedRitmNumber)
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(e => e.RelatedChangeNumber)
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("datetime2");

        builder.HasOne(e => e.Dependency)
            .WithMany(d => d.TraceEvents)
            .HasForeignKey(e => e.DependencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.DependencyId)
            .HasDatabaseName("IX_DependencyTraceEvents_DependencyId");

        builder.HasIndex(e => e.EventType)
            .HasDatabaseName("IX_DependencyTraceEvents_EventType");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_DependencyTraceEvents_CreatedAt");

        builder.HasIndex(e => e.RelatedIssueId)
            .HasDatabaseName("IX_DependencyTraceEvents_RelatedIssueId");

        builder.HasIndex(e => e.RelatedKnowledgeId)
            .HasDatabaseName("IX_DependencyTraceEvents_RelatedKnowledgeId");
    }
}
