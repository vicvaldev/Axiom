using System.Text.Json;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Axiom.Infrastructure.Data;

/// <summary>
/// Implements <see cref="IKnowledgeRepository"/> using a JSON file as the backing store.
/// Each operation reads the full file, mutates the in-memory list, and writes it back.
/// </summary>
public class JsonKnowledgeRepository : IKnowledgeRepository
{
    private readonly string _filePath;
    private readonly ILogger<JsonKnowledgeRepository> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonKnowledgeRepository"/> class.
    /// </summary>
    /// <param name="options">The JSON data options containing the file path for knowledge entries.</param>
    /// <param name="logger">The logger for recording errors and operations.</param>
    public JsonKnowledgeRepository(IOptions<JsonDataOptions> options, ILogger<JsonKnowledgeRepository> logger)
    {
        _filePath = options.Value.KnowledgeFilePath;
        _logger = logger;
    }

    /// <summary>Saves a knowledge entry to the JSON file, creating or updating as needed.</summary>
    /// <param name="entry">The knowledge entry to persist.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task SaveAsync(KnowledgeEntry entry, CancellationToken cancellationToken = default)
    {
        var entries = await LoadAllAsync(cancellationToken);
        var index = entries.FindIndex(e => e.Id == entry.Id);
        if (index >= 0)
            entries[index] = entry;
        else
            entries.Add(entry);

        await SaveAllAsync(entries, cancellationToken);
    }

    /// <summary>Retrieves a knowledge entry by its unique identifier.</summary>
    /// <param name="id">The unique identifier of the entry.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The matching entry, or <c>null</c> if not found.</returns>
    public async Task<KnowledgeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entries = await LoadAllAsync(cancellationToken);
        return entries.FirstOrDefault(e => e.Id == id);
    }

    /// <summary>Searches knowledge entries by matching the query against title, description, content, and tags.</summary>
    /// <param name="query">The search text to match.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A collection of entries matching the search criteria.</returns>
    public async Task<IEnumerable<KnowledgeEntry>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var entries = await LoadAllAsync(cancellationToken);
        var q = query.ToLowerInvariant();
        return entries.Where(e =>
            e.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            e.Description.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            e.Content.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            e.Tags.Any(t => t.Contains(q, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>Retrieves all knowledge entries from the JSON file.</summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A collection of all knowledge entries.</returns>
    public async Task<IEnumerable<KnowledgeEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await LoadAllAsync(cancellationToken);
    }

    /// <summary>Deletes a knowledge entry by its unique identifier.</summary>
    /// <param name="id">The unique identifier of the entry to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entries = await LoadAllAsync(cancellationToken);
        entries.RemoveAll(e => e.Id == id);
        await SaveAllAsync(entries, cancellationToken);
    }

    private async Task<List<KnowledgeEntry>> LoadAllAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
            return [];

        try
        {
            var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
            return JsonSerializer.Deserialize<List<KnowledgeEntry>>(json, _jsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load knowledge entries from {Path}", _filePath);
            return [];
        }
    }

    private async Task SaveAllAsync(List<KnowledgeEntry> entries, CancellationToken cancellationToken = default)
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(entries, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json, cancellationToken);
    }
}
