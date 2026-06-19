using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

public interface IIssueRepository
{
    Task SaveAsync(Issue issue, CancellationToken cancellationToken = default);
    Task<Issue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Issue>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Issue>> GetByEaiAsync(string eai, CancellationToken cancellationToken = default);
}
