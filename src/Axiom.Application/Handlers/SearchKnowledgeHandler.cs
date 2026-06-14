using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class SearchKnowledgeHandler : IRequestHandler<SearchKnowledgeQuery, IEnumerable<KnowledgeEntry>>
{
    private readonly IKnowledgeRepository _repository;

    public SearchKnowledgeHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<KnowledgeEntry>> Handle(SearchKnowledgeQuery request, CancellationToken cancellationToken)
    {
        return await _repository.SearchAsync(request.Query, cancellationToken);
    }
}
