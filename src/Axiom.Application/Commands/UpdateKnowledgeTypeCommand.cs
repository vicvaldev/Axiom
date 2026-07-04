using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record UpdateKnowledgeTypeCommand(
    long Id,
    string Code,
    string Name) : IRequest<KnowledgeType?>;
