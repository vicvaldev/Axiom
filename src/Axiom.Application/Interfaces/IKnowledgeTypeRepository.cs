using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

public interface IKnowledgeTypeRepository
{
    Task<KnowledgeType?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task SaveAsync(KnowledgeType knowledgeType, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
