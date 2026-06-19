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

    public async Task SaveAsync(Knowledge entry, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Knowledges
            .FirstOrDefaultAsync(e => e.KnowledgeId == entry.KnowledgeId, cancellationToken);

        if (existing is not null)
        {
            _context.Entry(existing).CurrentValues.SetValues(entry);
        }
        else
        {
            await _context.Knowledges.AddAsync(entry, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Knowledge?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Knowledges
            .AsNoTracking()
            .Include(k => k.System)
            .Include(k => k.CreatedBy)
            .Include(k => k.Type)
            .Include(k => k.State)
            .Include(k => k.KnowledgeKnowledgeTags)
                .ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(k => k.KnowledgeId == id, cancellationToken);
    }

    public async Task<IEnumerable<Knowledge>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Knowledges
            .AsNoTracking()
            .Include(k => k.System)
            .Include(k => k.CreatedBy)
            .Include(k => k.Type)
            .Include(k => k.State)
            .Include(k => k.KnowledgeKnowledgeTags)
                .ThenInclude(t => t.Tag)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Knowledge>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<Knowledge>();

        return await _context.Knowledges
            .AsNoTracking()
            .Include(k => k.System)
            .Include(k => k.CreatedBy)
            .Include(k => k.Type)
            .Include(k => k.State)
            .Include(k => k.KnowledgeKnowledgeTags)
                .ThenInclude(t => t.Tag)
            .Where(k =>
                k.Title.Contains(query) ||
                k.Summary.Contains(query) ||
                k.Content.Contains(query))
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Knowledges
            .FirstOrDefaultAsync(e => e.KnowledgeId == id, cancellationToken);

        if (entry is not null)
        {
            _context.Knowledges.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
