using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

public interface IIssueStateRepository
{
    Task<IssueState?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task SaveAsync(IssueState issueState, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
