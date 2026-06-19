using Axiom.Application.Dtos;
using MediatR;

namespace Axiom.Application.Queries;

public record SearchKnowledgeQuery(string Query) : IRequest<IEnumerable<KnowledgeDto>>;
