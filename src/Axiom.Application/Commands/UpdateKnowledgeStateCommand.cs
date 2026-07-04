using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record UpdateKnowledgeStateCommand(
    int Id,
    string Code,
    string Name) : IRequest<KnowledgeState?>;
