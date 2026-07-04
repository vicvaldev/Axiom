using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class EfComponentDependencyRepository : IComponentDependencyRepository
{
    private readonly AxiomDbContext _context;

    public EfComponentDependencyRepository(AxiomDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(ComponentDependency entry, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ComponentDependencies
            .FirstOrDefaultAsync(e => e.DependencyId == entry.DependencyId, cancellationToken);

        if (existing is not null)
        {
            _context.Entry(existing).CurrentValues.SetValues(entry);
        }
        else
        {
            await _context.ComponentDependencies.AddAsync(entry, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ComponentDependency?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ComponentDependencies
            .AsNoTracking()
            .Include(d => d.Source)
            .Include(d => d.Target)
            .FirstOrDefaultAsync(d => d.DependencyId == id, cancellationToken);
    }

    public async Task<IEnumerable<ComponentDependency>> GetByComponentIdAsync(Guid componentId, CancellationToken cancellationToken = default)
    {
        return await _context.ComponentDependencies
            .AsNoTracking()
            .Include(d => d.Source)
            .Include(d => d.Target)
            .Where(d => d.SourceComponentId == componentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ComponentDependency>> GetImpactedByComponentAsync(Guid componentId, CancellationToken cancellationToken = default)
    {
        return await _context.ComponentDependencies
            .AsNoTracking()
            .Include(d => d.Source)
            .Include(d => d.Target)
            .Where(d => d.TargetComponentId == componentId)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.ComponentDependencies
            .FirstOrDefaultAsync(e => e.DependencyId == id, cancellationToken);

        if (entry is not null)
        {
            _context.ComponentDependencies.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
