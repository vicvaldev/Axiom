using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class EfUserRepository : IUserRepository
{
    private readonly AxiomDbContext _context;

    public EfUserRepository(AxiomDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);
    }

    public async Task SaveAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);

        if (entry is not null)
        {
            _context.Users.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
