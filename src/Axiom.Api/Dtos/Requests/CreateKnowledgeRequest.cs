namespace Axiom.Api.Dtos.Requests;

public record CreateKnowledgeRequest(
    string Title,
    string Summary,
    string Content,
    long SystemId,
    Guid CreatedByUserId,
    long KnowledgeTypeId,
    int KnowledgeStateId,
    Guid? IssueId,
    List<string> Tags);
