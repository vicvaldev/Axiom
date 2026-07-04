using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

public interface ISystemComponentRepository
{
    Task SaveAsync(SystemComponent entry, CancellationToken cancellationToken = default);
}
