using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class EfIssueRepository : IIssueRepository
{
    private readonly AxiomDbContext _context;

    public EfIssueRepository(AxiomDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(Issue issue, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Issues
            .FirstOrDefaultAsync(i => i.IssueId == issue.IssueId, cancellationToken);

        if (existing is not null)
        {
            _context.Entry(existing).CurrentValues.SetValues(issue);
        }
        else
        {
            await _context.Issues.AddAsync(issue, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Issue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Issues
            .AsNoTracking()
            .Include(i => i.System)
            .Include(i => i.State)
            .Include(i => i.CreatedBy)
            .FirstOrDefaultAsync(i => i.IssueId == id, cancellationToken);
    }

    public async Task<IEnumerable<Issue>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Issues
            .AsNoTracking()
            .Include(i => i.System)
            .Include(i => i.State)
            .ToListAsync(cancellationToken);
    }
}
