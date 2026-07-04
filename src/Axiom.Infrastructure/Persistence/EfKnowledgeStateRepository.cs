using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class EfKnowledgeStateRepository : IKnowledgeStateRepository
{
    private readonly AxiomDbContext _context;

    public EfKnowledgeStateRepository(AxiomDbContext context)
    {
        _context = context;
    }

    public async Task<KnowledgeState?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeStates
            .FirstOrDefaultAsync(s => s.StateId == id, cancellationToken);
    }

    public async Task SaveAsync(KnowledgeState knowledgeState, CancellationToken cancellationToken = default)
    {
        _context.KnowledgeStates.Update(knowledgeState);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.KnowledgeStates
            .FirstOrDefaultAsync(s => s.StateId == id, cancellationToken);

        if (entry is not null)
        {
            _context.KnowledgeStates.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
