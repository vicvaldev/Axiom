using Axiom.Application.Dtos;
using MediatR;

namespace Axiom.Application.Queries;

public record AskQuery(string Question) : IRequest<IEnumerable<SearchResultDto>>;
