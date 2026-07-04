using MediatR;

namespace Axiom.Application.Commands;

public record DeleteSystemCommand(long Id) : IRequest<bool>;
