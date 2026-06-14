using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Queries;

public record GetCaseByIdQuery(Guid Id) : IRequest<CaseRecord?>;
