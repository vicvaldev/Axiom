using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class AxiomDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<AxiomSystem> Systems => Set<AxiomSystem>();
    public DbSet<KnowledgeTag> KnowledgeTags => Set<KnowledgeTag>();
    public DbSet<KnowledgeType> KnowledgeTypes => Set<KnowledgeType>();
    public DbSet<IssueState> IssueStates => Set<IssueState>();
    public DbSet<KnowledgeState> KnowledgeStates => Set<KnowledgeState>();
    public DbSet<Knowledge> Knowledges => Set<Knowledge>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<KnowledgeKnowledgeTag> KnowledgeKnowledgeTags => Set<KnowledgeKnowledgeTag>();

    public AxiomDbContext(DbContextOptions<AxiomDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AxiomDbContext).Assembly);
    }
}
