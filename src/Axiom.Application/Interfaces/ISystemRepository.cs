using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

public interface ISystemRepository
{
    Task<AxiomSystem?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task SaveAsync(AxiomSystem system, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
