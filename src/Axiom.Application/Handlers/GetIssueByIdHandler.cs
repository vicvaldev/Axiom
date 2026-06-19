using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class GetIssueByIdHandler : IRequestHandler<GetIssueByIdQuery, Issue?>
{
    private readonly IIssueRepository _repository;

    public GetIssueByIdHandler(IIssueRepository repository)
    {
        _repository = repository;
    }

    public async Task<Issue?> Handle(GetIssueByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken);
    }
}
