using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Queries;

public record SearchKnowledgeQuery(string Query) : IRequest<IEnumerable<KnowledgeEntry>>;
