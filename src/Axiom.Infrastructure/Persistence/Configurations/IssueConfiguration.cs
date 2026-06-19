using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

public class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
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
