using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using Axiom.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Axiom.Infrastructure.Persistence;

public class AxiomDbContext : DbContext
{
    public DbSet<KnowledgeEntry> KnowledgeEntries => Set<KnowledgeEntry>();
    public DbSet<CaseRecord> CaseRecords => Set<CaseRecord>();

    public AxiomDbContext(DbContextOptions<AxiomDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureKnowledgeEntry(modelBuilder);
        ConfigureCaseRecord(modelBuilder);
    }

    private static void ConfigureKnowledgeEntry(ModelBuilder modelBuilder)
    {
        var entry = modelBuilder.Entity<KnowledgeEntry>();
        entry.ToTable("KnowledgeEntries");

        entry.HasKey(e => e.Id);

        entry.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("Id");

        entry.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnType("nvarchar(500)");

        entry.Property(e => e.Description)
            .HasMaxLength(2000)
            .HasColumnType("nvarchar(2000)");

        entry.Property(e => e.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        entry.Property(e => e.System)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnType("nvarchar(200)")
            .HasConversion(new SystemNameConverter());

        entry.Property(e => e.Tags)
            .IsRequired()
            .HasColumnType("nvarchar(2000)")
            .HasConversion(new TagsConverter());

        entry.Property(e => e.CreatedAt)
            .HasColumnType("datetime2");

        entry.Property(e => e.UpdatedAt)
            .HasColumnType("datetime2");

        entry.Property(e => e.Author)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnType("nvarchar(200)");

        entry.Property(e => e.Type)
            .IsRequired()
            .HasColumnType("tinyint");

        entry.Property(e => e.Status)
            .IsRequired()
            .HasColumnType("tinyint")
            .HasConversion(new KnowledgeStatusValueConverter());

        entry.Property<DateTime?>("DeletedAt")
            .HasColumnType("datetime2");

        entry.HasQueryFilter(e => EF.Property<DateTime?>(e, "DeletedAt") == null);
    }

    private static void ConfigureCaseRecord(ModelBuilder modelBuilder)
    {
        var record = modelBuilder.Entity<CaseRecord>();
        record.ToTable("CaseRecords");

        record.HasKey(r => r.Id);

        record.Property(r => r.Id)
            .ValueGeneratedNever()
            .HasColumnName("Id");

        record.Property(r => r.RitmId)
            .HasMaxLength(100)
            .HasColumnType("nvarchar(100)")
            .HasConversion(new RitmIdConverter());

        record.Property(r => r.ChangeId)
            .HasMaxLength(100)
            .HasColumnType("nvarchar(100)");

        record.Property(r => r.System)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnType("nvarchar(200)")
            .HasConversion(new SystemNameConverter());

        record.Property(r => r.Problem)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        record.Property(r => r.Analysis)
            .HasColumnType("nvarchar(max)");

        record.Property(r => r.Resolution)
            .HasColumnType("nvarchar(max)");

        record.Property(r => r.LessonsLearned)
            .HasColumnType("nvarchar(max)");

        record.Property(r => r.CreatedAt)
            .HasColumnType("datetime2");

        record.Property(r => r.Status)
            .IsRequired()
            .HasColumnType("tinyint");

        record.Property<DateTime?>("DeletedAt")
            .HasColumnType("datetime2");

        record.HasQueryFilter(r => EF.Property<DateTime?>(r, "DeletedAt") == null);
    }
}

public class SystemNameConverter : ValueConverter<SystemName, string>
{
    public SystemNameConverter()
        : base(
            v => v.Value,
            v => new SystemName(v)) { }
}

public class RitmIdConverter : ValueConverter<RitmId?, string?>
{
    public RitmIdConverter()
        : base(
            v => v.HasValue ? v.Value.Value : null,
            v => string.IsNullOrWhiteSpace(v) ? null : new RitmId(v)) { }
}

public class KnowledgeStatusValueConverter : ValueConverter<KnowledgeStatusValue, byte>
{
    public KnowledgeStatusValueConverter()
        : base(
            v => (byte)v.Value,
            v => new KnowledgeStatusValue((KnowledgeStatus)v)) { }
}

public class TagsConverter : ValueConverter<List<string>, string>
{
    public TagsConverter()
        : base(
            v => string.Join(",", v),
            v => string.IsNullOrWhiteSpace(v)
                ? new List<string>()
                : v.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList()) { }
}
