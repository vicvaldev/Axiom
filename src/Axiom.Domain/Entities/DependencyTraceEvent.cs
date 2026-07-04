using Axiom.Domain.Enums;

namespace Axiom.Domain.Entities;

public class DependencyTraceEvent
{
    public Guid TraceEventId { get; private set; }
    public Guid DependencyId { get; private set; }
    public DependencyTraceEventType EventType { get; private set; }
    public string Description { get; private set; } = null!;
    public Guid? RelatedIssueId { get; private set; }
    public Guid? RelatedKnowledgeId { get; private set; }
    public string? RelatedRitmNumber { get; private set; }
    public string? RelatedChangeNumber { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ComponentDependency Dependency { get; private set; } = null!;

    private DependencyTraceEvent() { }

    public DependencyTraceEvent(
        Guid dependencyId,
        DependencyTraceEventType eventType,
        string description,
        Guid? relatedIssueId = null,
        Guid? relatedKnowledgeId = null,
        string? relatedRitmNumber = null,
        string? relatedChangeNumber = null,
        Guid? createdByUserId = null,
        Guid? traceEventId = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        TraceEventId = traceEventId ?? Guid.NewGuid();
        DependencyId = dependencyId;
        EventType = eventType;
        Description = description;
        RelatedIssueId = relatedIssueId;
        RelatedKnowledgeId = relatedKnowledgeId;
        RelatedRitmNumber = relatedRitmNumber;
        RelatedChangeNumber = relatedChangeNumber;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }
}
