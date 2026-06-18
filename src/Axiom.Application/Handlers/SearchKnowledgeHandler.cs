using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Handles the <see cref="SearchKnowledgeQuery"/> by searching knowledge entries matching the given text.
/// </summary>
public class SearchKnowledgeHandler : IRequestHandler<SearchKnowledgeQuery, IEnumerable<KnowledgeEntry>>
{
    private readonly IKnowledgeRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchKnowledgeHandler"/> class.
    /// </summary>
    /// <param name="repository">The knowledge repository used for searching.</param>
    public SearchKnowledgeHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the query by delegating to the repository's <see cref="IKnowledgeRepository.SearchAsync"/> method.
    /// </summary>
    /// <param name="request">The query containing the search text.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A collection of matching knowledge entries.</returns>
    public async Task<IEnumerable<KnowledgeEntry>> Handle(SearchKnowledgeQuery request, CancellationToken cancellationToken)
    {
        return await _repository.SearchAsync(request.Query, cancellationToken);
    }
}
