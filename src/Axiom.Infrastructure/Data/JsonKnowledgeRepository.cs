using System.Text.Json;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Axiom.Infrastructure.Data;

public class JsonKnowledgeRepository : IKnowledgeRepository
{
    private readonly string _filePath;
    private readonly ILogger<JsonKnowledgeRepository> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonKnowledgeRepository(IOptions<JsonDataOptions> options, ILogger<JsonKnowledgeRepository> logger)
    {
        _filePath = options.Value.KnowledgeFilePath;
        _logger = logger;
    }

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

    public async Task<KnowledgeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entries = await LoadAllAsync(cancellationToken);
        return entries.FirstOrDefault(e => e.Id == id);
    }

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

    public async Task<IEnumerable<KnowledgeEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await LoadAllAsync(cancellationToken);
    }

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
