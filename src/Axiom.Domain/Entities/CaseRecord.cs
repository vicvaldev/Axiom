using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Axiom.Domain.Enums;
using Axiom.Domain.ValueObjects;

namespace Axiom.Domain.Entities;

/// <summary>
/// Represents a case record entity that tracks operational incidents, problems, and their resolutions.
/// </summary>
public class CaseRecord
{
    /// <summary>Gets the unique identifier for the case record.</summary>
    [JsonInclude]
    public Guid Id { get; private set; }
    /// <summary>Gets the optional Request Item (RITM) identifier associated with this case.</summary>
    [JsonInclude]
    public RitmId? RitmId { get; private set; }
    /// <summary>Gets the optional change request identifier associated with this case.</summary>
    [JsonInclude]
    public string? ChangeId { get; private set; }
    /// <summary>Gets the system or application this case is related to.</summary>
    [JsonInclude]
    public SystemName System { get; private set; }
    /// <summary>Gets the description of the problem or issue reported.</summary>
    [JsonInclude]
    public string Problem { get; private set; } = null!;
    /// <summary>Gets the root cause analysis or investigation notes.</summary>
    [JsonInclude]
    public string Analysis { get; private set; } = null!;
    /// <summary>Gets the resolution steps or fix applied to address the problem.</summary>
    [JsonInclude]
    public string Resolution { get; private set; } = null!;
    /// <summary>Gets the lessons learned from handling this case.</summary>
    [JsonInclude]
    public string LessonsLearned { get; private set; } = null!;
    /// <summary>Gets the UTC timestamp when the case was created.</summary>
    [JsonInclude]
    public DateTime CreatedAt { get; private set; }
    /// <summary>Gets the current status of the case.</summary>
    [JsonInclude]
    public CaseStatus Status { get; private set; }

    /// <summary>
    /// Parameterless constructor required for JSON deserialization. Not intended for direct use.
    /// </summary>
    [JsonConstructor]
    private CaseRecord() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CaseRecord"/> class with the specified values.
    /// </summary>
    /// <param name="system">The associated system or application name.</param>
    /// <param name="problem">A description of the problem. Must not be null or whitespace.</param>
    /// <param name="analysis">Root cause analysis notes. Defaults to empty string if null.</param>
    /// <param name="resolution">Resolution steps applied. Defaults to empty string if null.</param>
    /// <param name="lessonsLearned">Lessons learned from the case. Defaults to empty string if null.</param>
    /// <param name="ritmId">An optional RITM identifier.</param>
    /// <param name="changeId">An optional change request identifier.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="problem"/> is null or whitespace.</exception>
    public CaseRecord(
        SystemName system,
        string problem,
        string analysis,
        string resolution,
        string lessonsLearned,
        RitmId? ritmId = null,
        string? changeId = null)
    {
        if (string.IsNullOrWhiteSpace(problem))
            throw new ArgumentException("Problem cannot be empty.", nameof(problem));

        Id = Guid.NewGuid();
        RitmId = ritmId;
        ChangeId = changeId;
        System = system;
        Problem = problem;
        Analysis = analysis ?? string.Empty;
        Resolution = resolution ?? string.Empty;
        LessonsLearned = lessonsLearned ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
        Status = CaseStatus.Open;
    }

    /// <summary>
    /// Updates the status of the case record to the specified value.
    /// </summary>
    /// <param name="newStatus">The new status to apply.</param>
    public void UpdateStatus(CaseStatus newStatus)
    {
        Status = newStatus;
    }
}
