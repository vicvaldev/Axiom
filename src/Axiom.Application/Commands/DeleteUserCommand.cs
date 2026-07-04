using MediatR;

namespace Axiom.Application.Commands;

public record DeleteUserCommand(Guid Id) : IRequest<bool>;
