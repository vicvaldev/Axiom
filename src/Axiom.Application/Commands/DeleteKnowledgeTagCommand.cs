using MediatR;

namespace Axiom.Application.Commands;

public record DeleteKnowledgeTagCommand(long Id) : IRequest<bool>;
