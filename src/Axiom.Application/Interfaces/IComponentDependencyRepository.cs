using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

public interface IComponentDependencyRepository
{
    Task SaveAsync(ComponentDependency entry, CancellationToken cancellationToken = default);
    Task<ComponentDependency?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ComponentDependency>> GetByComponentIdAsync(Guid componentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ComponentDependency>> GetImpactedByComponentAsync(Guid componentId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
