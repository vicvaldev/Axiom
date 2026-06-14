using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Queries;

public record GetKnowledgeByIdQuery(Guid Id) : IRequest<KnowledgeEntry?>;
