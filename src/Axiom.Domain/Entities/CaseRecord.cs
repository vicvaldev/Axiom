using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Axiom.Domain.Enums;
using Axiom.Domain.ValueObjects;

namespace Axiom.Domain.Entities;

public class CaseRecord
{
    [JsonInclude]
    public Guid Id { get; private set; }
    [JsonInclude]
    public RitmId? RitmId { get; private set; }
    [JsonInclude]
    public string? ChangeId { get; private set; }
    [JsonInclude]
    public SystemName System { get; private set; }
    [JsonInclude]
    public string Problem { get; private set; } = null!;
    [JsonInclude]
    public string Analysis { get; private set; } = null!;
    [JsonInclude]
    public string Resolution { get; private set; } = null!;
    [JsonInclude]
    public string LessonsLearned { get; private set; } = null!;
    [JsonInclude]
    public DateTime CreatedAt { get; private set; }
    [JsonInclude]
    public CaseStatus Status { get; private set; }

    [JsonConstructor]
    private CaseRecord() { }

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

    public void UpdateStatus(CaseStatus newStatus)
    {
        Status = newStatus;
    }
}
