using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

public interface IKnowledgeRepository
{
    Task SaveAsync(Knowledge entry, CancellationToken cancellationToken = default);
    Task<Knowledge?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Knowledge>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<IEnumerable<Knowledge>> GetAllAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
