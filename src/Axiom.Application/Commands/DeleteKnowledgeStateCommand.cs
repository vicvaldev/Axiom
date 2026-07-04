using MediatR;

namespace Axiom.Application.Commands;

public record DeleteKnowledgeStateCommand(int Id) : IRequest<bool>;
