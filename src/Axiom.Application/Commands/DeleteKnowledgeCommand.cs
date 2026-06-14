using MediatR;

namespace Axiom.Application.Commands;

public record DeleteKnowledgeCommand(Guid Id) : IRequest<bool>;
