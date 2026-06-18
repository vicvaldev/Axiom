using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Queries;

/// <summary>
/// Query to search knowledge entries by a free-text query.
/// </summary>
/// <param name="Query">The search text to match against entry titles, descriptions, content, and tags.</param>
public record SearchKnowledgeQuery(string Query) : IRequest<IEnumerable<KnowledgeEntry>>;
