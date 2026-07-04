using Axiom.Application.Dtos;
using MediatR;

namespace Axiom.Application.Queries;

public record ListComponentsBySystemQuery(long SystemId) : IRequest<IEnumerable<TechnicalComponentDto>>;
