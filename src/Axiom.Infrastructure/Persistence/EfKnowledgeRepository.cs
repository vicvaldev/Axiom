using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class EfKnowledgeRepository : IKnowledgeRepository
{
    private readonly AxiomDbContext _context;

    public EfKnowledgeRepository(AxiomDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(KnowledgeEntry entry, CancellationToken cancellationToken = default)
    {
        var existing = await _context.KnowledgeEntries
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == entry.Id, cancellationToken);

        if (existing is not null)
        {
            _context.Entry(existing).CurrentValues.SetValues(entry);
            _context.Entry(existing).Property("DeletedAt").CurrentValue = null;
        }
        else
        {
            await _context.KnowledgeEntries.AddAsync(entry, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<KnowledgeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeEntries
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<KnowledgeEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeEntries
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<KnowledgeEntry>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var dbResults = await _context.KnowledgeEntries
            .Where(e =>
                e.Title.Contains(query) ||
                e.Description.Contains(query) ||
                e.Content.Contains(query))
            .ToListAsync(cancellationToken);

        return dbResults
            .Where(e =>
                e.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.Content.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.Tags.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.KnowledgeEntries
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entry is not null)
        {
            _context.Entry(entry).Property("DeletedAt").CurrentValue = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
