using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record UpdateKnowledgeCommand(
    Guid Id,
    string Title,
    string Summary,
    string Content,
    long SystemId,
    long KnowledgeTypeId,
    int KnowledgeStateId,
    Guid? IssueId,
    List<string> Tags) : IRequest<Knowledge?>;
