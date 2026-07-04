using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class EfDependencyTraceEventRepository : IDependencyTraceEventRepository
{
    private readonly AxiomDbContext _context;

    public EfDependencyTraceEventRepository(AxiomDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DependencyTraceEvent entry, CancellationToken cancellationToken = default)
    {
        await _context.DependencyTraceEvents.AddAsync(entry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<DependencyTraceEvent>> GetByDependencyIdAsync(Guid dependencyId, CancellationToken cancellationToken = default)
    {
        return await _context.DependencyTraceEvents
            .AsNoTracking()
            .Where(e => e.DependencyId == dependencyId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
