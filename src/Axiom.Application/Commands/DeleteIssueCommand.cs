using MediatR;

namespace Axiom.Application.Commands;

public record DeleteIssueCommand(Guid Id) : IRequest<bool>;
