using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class GetCaseByIdHandler : IRequestHandler<GetCaseByIdQuery, CaseRecord?>
{
    private readonly ICaseRepository _repository;

    public GetCaseByIdHandler(ICaseRepository repository)
    {
        _repository = repository;
    }

    public async Task<CaseRecord?> Handle(GetCaseByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken);
    }
}
