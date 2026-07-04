using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class EfKnowledgeTypeRepository : IKnowledgeTypeRepository
{
    private readonly AxiomDbContext _context;

    public EfKnowledgeTypeRepository(AxiomDbContext context)
    {
        _context = context;
    }

    public async Task<KnowledgeType?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeTypes
            .FirstOrDefaultAsync(t => t.TypeId == id, cancellationToken);
    }

    public async Task SaveAsync(KnowledgeType knowledgeType, CancellationToken cancellationToken = default)
    {
        _context.KnowledgeTypes.Update(knowledgeType);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.KnowledgeTypes
            .FirstOrDefaultAsync(t => t.TypeId == id, cancellationToken);

        if (entry is not null)
        {
            _context.KnowledgeTypes.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
