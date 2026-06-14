using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using Axiom.Domain.ValueObjects;
using MediatR;

namespace Axiom.Application.Commands;

public record UpdateKnowledgeCommand(
    Guid Id,
    string Title,
    string Description,
    string Content,
    string System,
    List<string> Tags,
    KnowledgeType Type,
    KnowledgeStatusValue Status) : IRequest<KnowledgeEntry?>;
