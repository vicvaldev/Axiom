using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record CreateKnowledgeCommand(
    string Title,
    string Summary,
    string Content,
    long SystemId,
    Guid CreatedByUserId,
    long KnowledgeTypeId,
    int KnowledgeStateId,
    Guid? IssueId,
    List<string> Tags) : IRequest<Knowledge>;
