using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;

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
        var user = new User(email, name);
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        return user;
    }

    public async Task<AxiomSystem> CreateSystemAsync(string eai, string name, Guid ownerUserId, CancellationToken ct)
    {
        var system = new AxiomSystem(eai, name, ownerUserId);
        _context.Systems.Add(system);
        await _context.SaveChangesAsync(ct);
        return system;
    }

    public async Task<KnowledgeType> CreateKnowledgeTypeAsync(string code, string name, CancellationToken ct)
    {
        var type = new KnowledgeType(code, name);
        _context.KnowledgeTypes.Add(type);
        await _context.SaveChangesAsync(ct);
        return type;
    }

    public async Task<IssueState> CreateIssueStateAsync(string code, string name, CancellationToken ct)
    {
        var state = new IssueState(code, name);
        _context.IssueStates.Add(state);
        await _context.SaveChangesAsync(ct);
        return state;
    }

    public async Task<KnowledgeState> CreateKnowledgeStateAsync(string code, string name, CancellationToken ct)
    {
        var state = new KnowledgeState(code, name);
        _context.KnowledgeStates.Add(state);
        await _context.SaveChangesAsync(ct);
        return state;
    }
}
