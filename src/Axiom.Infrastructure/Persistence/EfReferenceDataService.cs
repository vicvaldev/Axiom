using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class EfReferenceDataService : IReferenceDataService
{
    private readonly AxiomDbContext _context;

    public EfReferenceDataService(AxiomDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UserDto>> ListUsersAsync(CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Email = u.Email,
                Name = u.Name
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SystemDto>> ListSystemsAsync(CancellationToken ct = default)
    {
        return await _context.Systems
            .AsNoTracking()
            .Include(s => s.Owner)
            .OrderBy(s => s.EAI)
            .Select(s => new SystemDto
            {
                SystemId = s.SystemId,
                EAI = s.EAI,
                Name = s.Name,
                OwnerUserId = s.OwnerUserId,
                OwnerName = s.Owner.Name
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ReferenceCodeDto>> ListKnowledgeTypesAsync(CancellationToken ct = default)
    {
        return await _context.KnowledgeTypes
            .AsNoTracking()
            .OrderBy(t => t.Code)
            .Select(t => new ReferenceCodeDto
            {
                Id = t.TypeId,
                Code = t.Code,
                Name = t.Name
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ReferenceCodeDto>> ListKnowledgeStatesAsync(CancellationToken ct = default)
    {
        return await _context.KnowledgeStates
            .AsNoTracking()
            .OrderBy(s => s.Code)
            .Select(s => new ReferenceCodeDto
            {
                Id = s.StateId,
                Code = s.Code,
                Name = s.Name
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ReferenceCodeDto>> ListIssueStatesAsync(CancellationToken ct = default)
    {
        return await _context.IssueStates
            .AsNoTracking()
            .OrderBy(s => s.Code)
            .Select(s => new ReferenceCodeDto
            {
                Id = s.StateId,
                Code = s.Code,
                Name = s.Name
            })
            .ToListAsync(ct);
    }

    public async Task<UserDto?> FindUserByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Email == email)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Email = u.Email,
                Name = u.Name
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<SystemDto?> FindSystemByEaiAsync(string eai, CancellationToken ct = default)
    {
        return await _context.Systems
            .AsNoTracking()
            .Include(s => s.Owner)
            .Where(s => s.EAI == eai)
            .Select(s => new SystemDto
            {
                SystemId = s.SystemId,
                EAI = s.EAI,
                Name = s.Name,
                OwnerUserId = s.OwnerUserId,
                OwnerName = s.Owner.Name
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ReferenceCodeDto?> FindKnowledgeTypeByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.KnowledgeTypes
            .AsNoTracking()
            .Where(t => t.Code == code)
            .Select(t => new ReferenceCodeDto
            {
                Id = t.TypeId,
                Code = t.Code,
                Name = t.Name
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ReferenceCodeDto?> FindKnowledgeStateByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.KnowledgeStates
            .AsNoTracking()
            .Where(s => s.Code == code)
            .Select(s => new ReferenceCodeDto
            {
                Id = s.StateId,
                Code = s.Code,
                Name = s.Name
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ReferenceCodeDto?> FindIssueStateByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.IssueStates
            .AsNoTracking()
            .Where(s => s.Code == code)
            .Select(s => new ReferenceCodeDto
            {
                Id = s.StateId,
                Code = s.Code,
                Name = s.Name
            })
            .FirstOrDefaultAsync(ct);
    }
}
