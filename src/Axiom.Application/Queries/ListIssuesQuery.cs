using Axiom.Application.Dtos;
using MediatR;

namespace Axiom.Application.Queries;

public record ListIssuesQuery(string? Eai = null) : IRequest<IEnumerable<IssueDto>>;
