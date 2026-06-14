using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class GetKnowledgeByIdHandler : IRequestHandler<GetKnowledgeByIdQuery, KnowledgeEntry?>
{
    private readonly IKnowledgeRepository _repository;

    public GetKnowledgeByIdHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    public async Task<KnowledgeEntry?> Handle(GetKnowledgeByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken);
    }
}
