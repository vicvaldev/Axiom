using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class EfCaseRepository : ICaseRepository
{
    private readonly AxiomDbContext _context;

    public EfCaseRepository(AxiomDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(CaseRecord record, CancellationToken cancellationToken = default)
    {
        var existing = await _context.CaseRecords
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == record.Id, cancellationToken);

        if (existing is not null)
        {
            _context.Entry(existing).CurrentValues.SetValues(record);
            _context.Entry(existing).Property("DeletedAt").CurrentValue = null;
        }
        else
        {
            await _context.CaseRecords.AddAsync(record, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<CaseRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CaseRecords
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<CaseRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CaseRecords
            .ToListAsync(cancellationToken);
    }
}
