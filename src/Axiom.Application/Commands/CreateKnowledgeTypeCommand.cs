using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record CreateKnowledgeTypeCommand(
    string Code,
    string Name) : IRequest<KnowledgeType>;
