using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class EfKnowledgeTagRepository : IKnowledgeTagRepository
{
    private readonly AxiomDbContext _context;

    public EfKnowledgeTagRepository(AxiomDbContext context)
    {
        _context = context;
    }

    public async Task<KnowledgeTag?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeTags
            .FirstOrDefaultAsync(t => t.KnowledgeTagId == id, cancellationToken);
    }

    public async Task SaveAsync(KnowledgeTag tag, CancellationToken cancellationToken = default)
    {
        _context.KnowledgeTags.Update(tag);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<KnowledgeTag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeTags
            .OrderBy(t => t.TagName)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.KnowledgeTags
            .FirstOrDefaultAsync(t => t.KnowledgeTagId == id, cancellationToken);

        if (entry is not null)
        {
            _context.KnowledgeTags.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
