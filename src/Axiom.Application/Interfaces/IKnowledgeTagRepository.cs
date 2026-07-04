using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

public interface IKnowledgeTagRepository
{
    Task<KnowledgeTag?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task SaveAsync(KnowledgeTag tag, CancellationToken cancellationToken = default);
    Task<List<KnowledgeTag>> GetAllAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
