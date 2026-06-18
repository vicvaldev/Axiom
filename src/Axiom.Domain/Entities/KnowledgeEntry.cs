using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Axiom.Domain.Enums;
using Axiom.Domain.ValueObjects;

namespace Axiom.Domain.Entities;

/// <summary>
/// Represents a knowledge entry entity, containing documentation, runbooks, troubleshooting guides, or reference material.
/// </summary>
public class KnowledgeEntry
{
    /// <summary>Gets the unique identifier for the knowledge entry.</summary>
    [JsonInclude]
    public Guid Id { get; private set; }
    /// <summary>Gets the title of the knowledge entry.</summary>
    [JsonInclude]
    public string Title { get; private set; } = null!;
    /// <summary>Gets a short description summarizing the knowledge entry.</summary>
    [JsonInclude]
    public string Description { get; private set; } = null!;
    /// <summary>Gets the main body content of the knowledge entry.</summary>
    [JsonInclude]
    public string Content { get; private set; } = null!;
    /// <summary>Gets the system or application this entry is associated with.</summary>
    [JsonInclude]
    public SystemName System { get; private set; }
    /// <summary>Gets the list of tags used for categorizing and searching this entry.</summary>
    [JsonInclude]
    public List<string> Tags { get; private set; } = null!;
    /// <summary>Gets the UTC timestamp when the entry was created.</summary>
    [JsonInclude]
    public DateTime CreatedAt { get; private set; }
    /// <summary>Gets the UTC timestamp when the entry was last updated.</summary>
    [JsonInclude]
    public DateTime UpdatedAt { get; private set; }
    /// <summary>Gets the name of the author who created or last modified the entry.</summary>
    [JsonInclude]
    public string Author { get; private set; } = null!;
    /// <summary>Gets the content type classification of this entry.</summary>
    [JsonInclude]
    public KnowledgeType Type { get; private set; }
    /// <summary>Gets the current lifecycle status of the entry.</summary>
    [JsonInclude]
    public KnowledgeStatusValue Status { get; private set; }

    /// <summary>
    /// Parameterless constructor required for JSON deserialization. Not intended for direct use.
    /// </summary>
    [JsonConstructor]
    private KnowledgeEntry() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="KnowledgeEntry"/> class with the specified values.
    /// </summary>
    /// <param name="title">The title of the entry. Must not be null or whitespace.</param>
    /// <param name="description">An optional summary description. Defaults to empty string if null.</param>
    /// <param name="content">The main body content. Must not be null or whitespace.</param>
    /// <param name="system">The associated system or application name.</param>
    /// <param name="tags">A list of tags for categorization. Defaults to an empty list if null.</param>
    /// <param name="author">The name of the author. Defaults to "unknown" if null.</param>
    /// <param name="type">The content type classification.</param>
    /// <param name="status">The initial lifecycle status.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="title"/> or <paramref name="content"/> is null or whitespace.</exception>
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

    /// <summary>
    /// Updates the modifiable properties of the knowledge entry and refreshes the <see cref="UpdatedAt"/> timestamp.
    /// </summary>
    /// <param name="title">The new title. Must not be null or whitespace.</param>
    /// <param name="description">The new description. Defaults to empty string if null.</param>
    /// <param name="content">The new content. Must not be null or whitespace.</param>
    /// <param name="system">The new associated system or application name.</param>
    /// <param name="tags">The new list of tags. Defaults to an empty list if null.</param>
    /// <param name="type">The new content type classification.</param>
    /// <param name="status">The new lifecycle status.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="title"/> or <paramref name="content"/> is null or whitespace.</exception>
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
