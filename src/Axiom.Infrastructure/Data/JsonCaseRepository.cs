using System.Text.Json;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Axiom.Infrastructure.Data;

public class JsonCaseRepository : ICaseRepository
{
    private readonly string _filePath;
    private readonly ILogger<JsonCaseRepository> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonCaseRepository(IOptions<JsonDataOptions> options, ILogger<JsonCaseRepository> logger)
    {
        _filePath = options.Value.CasesFilePath;
        _logger = logger;
    }

    public async Task SaveAsync(CaseRecord record, CancellationToken cancellationToken = default)
    {
        var records = await LoadAllAsync(cancellationToken);
        var index = records.FindIndex(r => r.Id == record.Id);
        if (index >= 0)
            records[index] = record;
        else
            records.Add(record);

        await SaveAllAsync(records, cancellationToken);
    }

    public async Task<CaseRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var records = await LoadAllAsync(cancellationToken);
        return records.FirstOrDefault(r => r.Id == id);
    }

    public async Task<IEnumerable<CaseRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await LoadAllAsync(cancellationToken);
    }

    private async Task<List<CaseRecord>> LoadAllAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
            return [];

        try
        {
            var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
            return JsonSerializer.Deserialize<List<CaseRecord>>(json, _jsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load case records from {Path}", _filePath);
            return [];
        }
    }

    private async Task SaveAllAsync(List<CaseRecord> records, CancellationToken cancellationToken = default)
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(records, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json, cancellationToken);
    }
}
