using System.Collections.Concurrent;
using System.Text.Json;
using Axiom.Application.Interfaces;

namespace Axiom.Infrastructure.Persistence;

public class JsonStore : IJsonStore
{
    private readonly string _basePath;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public JsonStore()
    {
        _basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".axiom", "data");
        Directory.CreateDirectory(_basePath);
    }

    public JsonStore(string basePath)
    {
        _basePath = basePath;
        Directory.CreateDirectory(_basePath);
    }

    public string BasePath => _basePath;

    public async Task AppendAsync<T>(string entityName, T entry, CancellationToken ct = default)
    {
        var semaphore = _locks.GetOrAdd(entityName, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);
        try
        {
            var filePath = GetFilePath(entityName);
            List<T> entries;

            if (File.Exists(filePath))
            {
                var existingJson = await File.ReadAllTextAsync(filePath, ct);
                entries = JsonSerializer.Deserialize<List<T>>(existingJson, JsonOptions) ?? [];
            }
            else
            {
                entries = [];
            }

            entries.Add(entry);
            var json = JsonSerializer.Serialize(entries, JsonOptions);
            await File.WriteAllTextAsync(filePath, json, ct);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<List<T>> ReadAllAsync<T>(string entityName, CancellationToken ct = default)
    {
        var filePath = GetFilePath(entityName);
        if (!File.Exists(filePath))
            return [];

        var json = await File.ReadAllTextAsync(filePath, ct);
        return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? [];
    }

    public async Task<T?> FindByIdAsync<T>(string entityName, Func<T, bool> predicate, CancellationToken ct = default)
    {
        var entries = await ReadAllAsync<T>(entityName, ct);
        return entries.FirstOrDefault(predicate);
    }

    public async Task<bool> ExistsAsync(string entityName, CancellationToken ct = default)
    {
        var filePath = GetFilePath(entityName);
        return File.Exists(filePath) && (await ReadAllAsync<object>(entityName, ct)).Count > 0;
    }

    public async Task<bool> UpdateAsync<T>(string entityName, Func<T, bool> predicate, T updatedEntry, CancellationToken ct = default)
    {
        var semaphore = _locks.GetOrAdd(entityName, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);
        try
        {
            var entries = await ReadAllAsync<T>(entityName, ct);
            var index = entries.FindIndex(e => predicate(e));
            if (index < 0)
                return false;

            entries[index] = updatedEntry;
            var filePath = GetFilePath(entityName);
            var json = JsonSerializer.Serialize(entries, JsonOptions);
            await File.WriteAllTextAsync(filePath, json, ct);
            return true;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private string GetFilePath(string entityName)
    {
        return Path.Combine(_basePath, $"{entityName.ToLowerInvariant()}.json");
    }
}
