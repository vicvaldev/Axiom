using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using MediatR;

namespace Axiom.Application.Commands;

public record CreateDependencyTraceEventCommand(
    Guid DependencyId,
    DependencyTraceEventType EventType,
    string Description,
    Guid? RelatedIssueId = null,
    Guid? RelatedKnowledgeId = null,
    string? RelatedRitmNumber = null,
    string? RelatedChangeNumber = null,
    Guid? CreatedByUserId = null) : IRequest<DependencyTraceEvent>;
