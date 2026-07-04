using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record UpdateIssueCommand(
    Guid Id,
    string Summary,
    string Problem,
    string? Analysis,
    string? Resolution,
    long SystemId,
    int StateId,
    string? RitmNumber,
    string? IncidentNumber) : IRequest<Issue?>;
