using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

public interface ITechnicalComponentRepository
{
    Task SaveAsync(TechnicalComponent entry, CancellationToken cancellationToken = default);
    Task<TechnicalComponent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TechnicalComponent>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TechnicalComponent>> GetBySystemIdAsync(long systemId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
