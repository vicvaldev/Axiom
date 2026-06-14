using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

public interface ICaseRepository
{
    Task SaveAsync(CaseRecord record, CancellationToken cancellationToken = default);
    Task<CaseRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CaseRecord>> GetAllAsync(CancellationToken cancellationToken = default);
}
