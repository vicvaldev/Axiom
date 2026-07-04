using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record CreateIssueStateCommand(
    string Code,
    string Name) : IRequest<IssueState>;
