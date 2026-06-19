using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class EfStartupService : IStartupService
{
    private readonly AxiomDbContext _context;

    public EfStartupService(AxiomDbContext context)
    {
        _context = context;
    }

    public async Task<User> CreateUserAsync(string email, string name, CancellationToken ct)
    {
        var existing = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);
        if (existing is not null)
        {
            return existing;
        }

        var user = new User(email, name);
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        return user;
    }

    public async Task<AxiomSystem> CreateSystemAsync(string eai, string name, Guid ownerUserId, CancellationToken ct)
    {
        var existing = await _context.Systems
            .FirstOrDefaultAsync(s => s.EAI == eai, ct);
        if (existing is not null)
        {
            return existing;
        }

        var system = new AxiomSystem(eai, name, ownerUserId);
        _context.Systems.Add(system);
        await _context.SaveChangesAsync(ct);
        return system;
    }

    public async Task<KnowledgeType> CreateKnowledgeTypeAsync(string code, string name, CancellationToken ct)
    {
        var existing = await _context.KnowledgeTypes
            .FirstOrDefaultAsync(t => t.Code == code, ct);
        if (existing is not null)
        {
            return existing;
        }

        var type = new KnowledgeType(code, name);
        _context.KnowledgeTypes.Add(type);
        await _context.SaveChangesAsync(ct);
        return type;
    }

    public async Task<IssueState> CreateIssueStateAsync(string code, string name, CancellationToken ct)
    {
        var existing = await _context.IssueStates
            .FirstOrDefaultAsync(s => s.Code == code, ct);
        if (existing is not null)
        {
            return existing;
        }

        var state = new IssueState(code, name);
        _context.IssueStates.Add(state);
        await _context.SaveChangesAsync(ct);
        return state;
    }

    public async Task<KnowledgeState> CreateKnowledgeStateAsync(string code, string name, CancellationToken ct)
    {
        var existing = await _context.KnowledgeStates
            .FirstOrDefaultAsync(s => s.Code == code, ct);
        if (existing is not null)
        {
            return existing;
        }

        var state = new KnowledgeState(code, name);
        _context.KnowledgeStates.Add(state);
        await _context.SaveChangesAsync(ct);
        return state;
    }
}
