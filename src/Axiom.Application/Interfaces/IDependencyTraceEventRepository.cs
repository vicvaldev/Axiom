using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

public interface IDependencyTraceEventRepository
{
    Task AddAsync(DependencyTraceEvent entry, CancellationToken cancellationToken = default);
    Task<IEnumerable<DependencyTraceEvent>> GetByDependencyIdAsync(Guid dependencyId, CancellationToken cancellationToken = default);
}
