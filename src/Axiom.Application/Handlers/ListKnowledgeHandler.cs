using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class ListKnowledgeHandler : IRequestHandler<ListKnowledgeQuery, IEnumerable<KnowledgeEntry>>
{
    private readonly IKnowledgeRepository _repository;

    public ListKnowledgeHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<KnowledgeEntry>> Handle(ListKnowledgeQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}
