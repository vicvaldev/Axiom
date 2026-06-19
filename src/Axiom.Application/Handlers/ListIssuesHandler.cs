using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using MediatR;

namespace Axiom.Application.Handlers;

public class ListIssuesHandler : IRequestHandler<ListIssuesQuery, IEnumerable<IssueDto>>
{
    private readonly IIssueRepository _repository;

    public ListIssuesHandler(IIssueRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<IssueDto>> Handle(ListIssuesQuery request, CancellationToken cancellationToken)
    {
        var issues = string.IsNullOrWhiteSpace(request.Eai)
            ? await _repository.GetAllAsync(cancellationToken)
            : await _repository.GetByEaiAsync(request.Eai, cancellationToken);

        return issues.Select(i => new IssueDto
        {
            IssueId = i.IssueId,
            Summary = i.Summary,
            SystemName = i.System?.Name ?? string.Empty,
            StateName = i.State?.Name ?? string.Empty,
            RitmNumber = i.RitmNumber,
            IncidentNumber = i.IncidentNumber,
            CreatedAt = i.CreatedAt,
            ResolvedAt = i.ResolvedAt
        });
    }
}
