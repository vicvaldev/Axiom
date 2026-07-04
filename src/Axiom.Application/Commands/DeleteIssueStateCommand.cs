using MediatR;

namespace Axiom.Application.Commands;

public record DeleteIssueStateCommand(int Id) : IRequest<bool>;
