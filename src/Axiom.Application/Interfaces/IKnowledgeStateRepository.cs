using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

public interface IKnowledgeStateRepository
{
    Task<KnowledgeState?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task SaveAsync(KnowledgeState knowledgeState, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
