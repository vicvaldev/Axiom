using Axiom.Application.Dtos;
using MediatR;

namespace Axiom.Application.Queries;

public record GetComponentByIdQuery(Guid ComponentId) : IRequest<TechnicalComponentDto?>;
