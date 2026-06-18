using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

/// <summary>
/// Defines the contract for persisting and retrieving <see cref="KnowledgeEntry"/> records.
/// </summary>
public interface IKnowledgeRepository
{
    /// <summary>Saves a knowledge entry (creates or updates) to the underlying store.</summary>
    /// <param name="entry">The knowledge entry to persist.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task SaveAsync(KnowledgeEntry entry, CancellationToken cancellationToken = default);
    /// <summary>Retrieves a knowledge entry by its unique identifier.</summary>
    /// <param name="id">The unique identifier of the entry.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The matching entry, or <c>null</c> if not found.</returns>
    Task<KnowledgeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>Searches knowledge entries by matching the query against title, description, content, and tags.</summary>
    /// <param name="query">The search text to match.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A collection of entries matching the search criteria.</returns>
    Task<IEnumerable<KnowledgeEntry>> SearchAsync(string query, CancellationToken cancellationToken = default);
    /// <summary>Retrieves all knowledge entries from the store.</summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A collection of all knowledge entries.</returns>
    Task<IEnumerable<KnowledgeEntry>> GetAllAsync(CancellationToken cancellationToken = default);
    /// <summary>Deletes a knowledge entry by its unique identifier.</summary>
    /// <param name="id">The unique identifier of the entry to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
