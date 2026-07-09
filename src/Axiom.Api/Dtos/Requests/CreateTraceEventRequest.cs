namespace Axiom.Api.Dtos.Requests;

public record CreateTraceEventRequest(
    Guid DependencyId,
    string EventType,
    string Description,
    Guid? IssueId,
    Guid? KnowledgeId,
    string? RitmNumber,
    string? ChangeNumber,
    Guid? CreatedByUserId);
