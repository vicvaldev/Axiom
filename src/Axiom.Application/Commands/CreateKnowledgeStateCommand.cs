using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record CreateKnowledgeStateCommand(
    string Code,
    string Name) : IRequest<KnowledgeState>;
