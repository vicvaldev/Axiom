using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

public interface IStartupService
{
    Task<User> CreateUserAsync(string email, string name, CancellationToken ct);
    Task<AxiomSystem> CreateSystemAsync(string eai, string name, Guid ownerUserId, CancellationToken ct);
    Task<KnowledgeType> CreateKnowledgeTypeAsync(string code, string name, CancellationToken ct);
    Task<IssueState> CreateIssueStateAsync(string code, string name, CancellationToken ct);
    Task<KnowledgeState> CreateKnowledgeStateAsync(string code, string name, CancellationToken ct);
}
