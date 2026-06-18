using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Queries;

/// <summary>
/// Query to retrieve all knowledge entries.
/// </summary>
public record ListKnowledgeQuery : IRequest<IEnumerable<KnowledgeEntry>>;
