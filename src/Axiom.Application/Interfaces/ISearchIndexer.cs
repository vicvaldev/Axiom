namespace Axiom.Application.Interfaces;

public interface ISearchIndexer
{
    Task IndexKnowledgeAsync(Guid id, CancellationToken ct = default);
    Task IndexIssueAsync(Guid id, CancellationToken ct = default);
    Task DeleteKnowledgeAsync(Guid id, CancellationToken ct = default);
    Task DeleteIssueAsync(Guid id, CancellationToken ct = default);
    Task ReindexAllAsync(CancellationToken ct = default);
}
