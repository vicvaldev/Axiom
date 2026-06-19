using Axiom.Application.Dtos;
using MediatR;

namespace Axiom.Application.Queries;

public record ListIssuesQuery : IRequest<IEnumerable<IssueDto>>;
