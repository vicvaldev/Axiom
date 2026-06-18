using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Queries;

/// <summary>
/// Query to retrieve a knowledge entry by its unique identifier.
/// </summary>
/// <param name="Id">The unique identifier of the knowledge entry.</param>
public record GetKnowledgeByIdQuery(Guid Id) : IRequest<KnowledgeEntry?>;
