namespace Axiom.Application.Dtos;

public class DependencyTraceEventDto
{
    public Guid TraceEventId { get; init; }
    public Guid DependencyId { get; init; }
    public string EventType { get; init; } = null!;
    public string Description { get; init; } = null!;
    public Guid? RelatedIssueId { get; init; }
    public Guid? RelatedKnowledgeId { get; init; }
    public string? RelatedRitmNumber { get; init; }
    public string? RelatedChangeNumber { get; init; }
    public Guid? CreatedByUserId { get; init; }
    public DateTime CreatedAt { get; init; }
}
