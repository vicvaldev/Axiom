using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Axiom.Infrastructure.Persistence.Configurations;

public class IssueStateConfiguration : IEntityTypeConfiguration<IssueState>
{
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
