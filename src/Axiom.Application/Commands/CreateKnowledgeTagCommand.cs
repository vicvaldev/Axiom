using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record CreateKnowledgeTagCommand(string TagName) : IRequest<KnowledgeTag>;
