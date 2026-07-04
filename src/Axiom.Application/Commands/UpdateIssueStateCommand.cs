using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record UpdateIssueStateCommand(
    int Id,
    string Code,
    string Name) : IRequest<IssueState?>;
