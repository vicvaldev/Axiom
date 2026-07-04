using Axiom.Application.Dtos;
using MediatR;

namespace Axiom.Application.Queries;

public record ListDependenciesByComponentQuery(Guid ComponentId) : IRequest<IEnumerable<ComponentDependencyDto>>;
