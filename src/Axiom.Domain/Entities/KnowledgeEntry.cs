using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Axiom.Domain.Enums;
using Axiom.Domain.ValueObjects;

namespace Axiom.Domain.Entities;

public class KnowledgeEntry
{
    [JsonInclude]
    public Guid Id { get; private set; }
    [JsonInclude]
    public string Title { get; private set; } = null!;
    [JsonInclude]
    public string Description { get; private set; } = null!;
    [JsonInclude]
    public string Content { get; private set; } = null!;
    [JsonInclude]
    public SystemName System { get; private set; }
    [JsonInclude]
    public List<string> Tags { get; private set; } = null!;
    [JsonInclude]
    public DateTime CreatedAt { get; private set; }
    [JsonInclude]
    public DateTime UpdatedAt { get; private set; }
    [JsonInclude]
    public string Author { get; private set; } = null!;
    [JsonInclude]
    public KnowledgeType Type { get; private set; }
    [JsonInclude]
    public KnowledgeStatusValue Status { get; private set; }

    [JsonConstructor]
    private KnowledgeEntry() { }

    public KnowledgeEntry(
        string title,
        string description,
        string content,
        SystemName system,
        List<string> tags,
        string author,
        KnowledgeType type,
        KnowledgeStatusValue status)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        Id = Guid.NewGuid();
        Title = title;
        Description = description ?? string.Empty;
        Content = content;
        System = system;
        Tags = tags ?? [];
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Author = author ?? "unknown";
        Type = type;
        Status = status;
    }

    public void Update(
        string title,
        string description,
        string content,
        SystemName system,
        List<string> tags,
        KnowledgeType type,
        KnowledgeStatusValue status)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        Title = title;
        Description = description ?? string.Empty;
        Content = content;
        System = system;
        Tags = tags ?? [];
        Type = type;
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}
