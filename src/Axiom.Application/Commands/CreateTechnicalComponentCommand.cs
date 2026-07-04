using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using MediatR;

namespace Axiom.Application.Commands;

public record CreateTechnicalComponentCommand(
    string Name,
    string TechnicalName,
    ComponentType ComponentType,
    TargetEnvironment Environment,
    Criticality Criticality,
    long SystemId,
    string? Description = null) : IRequest<TechnicalComponent>;
