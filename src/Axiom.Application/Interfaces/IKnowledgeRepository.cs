using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

public interface IKnowledgeRepository
{
    Task SaveAsync(KnowledgeEntry entry, CancellationToken cancellationToken = default);
    Task<KnowledgeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<KnowledgeEntry>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<IEnumerable<KnowledgeEntry>> GetAllAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
