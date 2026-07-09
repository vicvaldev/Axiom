namespace Axiom.Api.Dtos.Requests;

public record UpdateKnowledgeRequest(
    string Title,
    string Summary,
    string Content,
    long SystemId,
    long KnowledgeTypeId,
    int KnowledgeStateId,
    Guid? IssueId,
    List<string> Tags);
