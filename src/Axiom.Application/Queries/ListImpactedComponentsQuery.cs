using Axiom.Application.Dtos;
using MediatR;

namespace Axiom.Application.Queries;

public record ListImpactedComponentsQuery(Guid ComponentId) : IRequest<IEnumerable<ComponentDependencyDto>>;
