using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;

namespace Axiom.Infrastructure.Persistence;

public class EfSystemComponentRepository : ISystemComponentRepository
{
    private readonly AxiomDbContext _context;

    public EfSystemComponentRepository(AxiomDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(SystemComponent entry, CancellationToken cancellationToken = default)
    {
        await _context.SystemComponents.AddAsync(entry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
