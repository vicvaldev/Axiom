namespace Axiom.Application.Interfaces;

public interface IJsonStore
{
    string BasePath { get; }
    Task AppendAsync<T>(string entityName, T entry, CancellationToken ct = default);
    Task<List<T>> ReadAllAsync<T>(string entityName, CancellationToken ct = default);
    Task<T?> FindByIdAsync<T>(string entityName, Func<T, bool> predicate, CancellationToken ct = default);
    Task<bool> ExistsAsync(string entityName, CancellationToken ct = default);
    Task<bool> UpdateAsync<T>(string entityName, Func<T, bool> predicate, T updatedEntry, CancellationToken ct = default);
}
