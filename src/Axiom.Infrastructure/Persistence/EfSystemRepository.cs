using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class EfSystemRepository : ISystemRepository
{
    private readonly AxiomDbContext _context;

    public EfSystemRepository(AxiomDbContext context)
    {
        _context = context;
    }

    public async Task<AxiomSystem?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Systems
            .FirstOrDefaultAsync(s => s.SystemId == id, cancellationToken);
    }

    public async Task SaveAsync(AxiomSystem system, CancellationToken cancellationToken = default)
    {
        _context.Systems.Update(system);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Systems
            .FirstOrDefaultAsync(s => s.SystemId == id, cancellationToken);

        if (entry is not null)
        {
            _context.Systems.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
