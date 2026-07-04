using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record CreateIssueCommand(
    string Summary,
    long SystemId,
    string Problem,
    string Analysis,
    string Resolution,
    int StateId,
    Guid CreatedByUserId,
    string? RitmNumber,
    string? IncidentNumber,
    Guid? IssueId = null) : IRequest<Issue>;
