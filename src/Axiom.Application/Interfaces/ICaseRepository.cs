using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

/// <summary>
/// Defines the contract for persisting and retrieving <see cref="CaseRecord"/> entities.
/// </summary>
public interface ICaseRepository
{
    /// <summary>Saves a case record (creates or updates) to the underlying store.</summary>
    /// <param name="record">The case record to persist.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task SaveAsync(CaseRecord record, CancellationToken cancellationToken = default);
    /// <summary>Retrieves a case record by its unique identifier.</summary>
    /// <param name="id">The unique identifier of the case record.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The matching case record, or <c>null</c> if not found.</returns>
    Task<CaseRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>Retrieves all case records from the store.</summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A collection of all case records.</returns>
    Task<IEnumerable<CaseRecord>> GetAllAsync(CancellationToken cancellationToken = default);
}
