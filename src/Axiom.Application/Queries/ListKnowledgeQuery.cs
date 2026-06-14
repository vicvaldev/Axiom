using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Queries;

public record ListKnowledgeQuery : IRequest<IEnumerable<KnowledgeEntry>>;
