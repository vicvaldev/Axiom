using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Queries;

/// <summary>
/// Query to retrieve a case record by its unique identifier.
/// </summary>
/// <param name="Id">The unique identifier of the case record.</param>
public record GetCaseByIdQuery(Guid Id) : IRequest<CaseRecord?>;
