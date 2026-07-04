using MediatR;

namespace Axiom.Application.Commands;

public record DeleteKnowledgeTypeCommand(long Id) : IRequest<bool>;
