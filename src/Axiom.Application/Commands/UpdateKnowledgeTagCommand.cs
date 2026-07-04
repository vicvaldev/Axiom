using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record UpdateKnowledgeTagCommand(
    long Id,
    string TagName) : IRequest<KnowledgeTag?>;
