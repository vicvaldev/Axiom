using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class EfIssueStateRepository : IIssueStateRepository
{
    private readonly AxiomDbContext _context;

    public EfIssueStateRepository(AxiomDbContext context)
    {
        _context = context;
    }

    public async Task<IssueState?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.IssueStates
            .FirstOrDefaultAsync(s => s.StateId == id, cancellationToken);
    }

    public async Task SaveAsync(IssueState issueState, CancellationToken cancellationToken = default)
    {
        _context.IssueStates.Update(issueState);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.IssueStates
            .FirstOrDefaultAsync(s => s.StateId == id, cancellationToken);

        if (entry is not null)
        {
            _context.IssueStates.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
