using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class EfTechnicalComponentRepository : ITechnicalComponentRepository
{
    private readonly AxiomDbContext _context;

    public EfTechnicalComponentRepository(AxiomDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(TechnicalComponent entry, CancellationToken cancellationToken = default)
    {
        var existing = await _context.TechnicalComponents
            .FirstOrDefaultAsync(e => e.ComponentId == entry.ComponentId, cancellationToken);

        if (existing is not null)
        {
            _context.Entry(existing).CurrentValues.SetValues(entry);
        }
        else
        {
            await _context.TechnicalComponents.AddAsync(entry, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TechnicalComponent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TechnicalComponents
            .AsNoTracking()
            .Include(c => c.System)
            .FirstOrDefaultAsync(c => c.ComponentId == id, cancellationToken);
    }

    public async Task<IEnumerable<TechnicalComponent>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TechnicalComponents
            .AsNoTracking()
            .Include(c => c.System)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TechnicalComponent>> GetBySystemIdAsync(long systemId, CancellationToken cancellationToken = default)
    {
        return await _context.TechnicalComponents
            .AsNoTracking()
            .Include(c => c.System)
            .Where(c => c.SystemId == systemId)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.TechnicalComponents
            .FirstOrDefaultAsync(e => e.ComponentId == id, cancellationToken);

        if (entry is not null)
        {
            _context.TechnicalComponents.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
