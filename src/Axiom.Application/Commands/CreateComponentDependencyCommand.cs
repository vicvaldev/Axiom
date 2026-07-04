using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using MediatR;

namespace Axiom.Application.Commands;

public record CreateComponentDependencyCommand(
    Guid SourceComponentId,
    Guid TargetComponentId,
    DependencyType DependencyType,
    Criticality Criticality,
    DependencyStatus Status,
    string? Description = null) : IRequest<ComponentDependency>;
