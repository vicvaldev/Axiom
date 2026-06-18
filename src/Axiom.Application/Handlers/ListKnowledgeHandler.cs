using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Handles the <see cref="ListKnowledgeQuery"/> by retrieving all knowledge entries from the repository.
/// </summary>
public class ListKnowledgeHandler : IRequestHandler<ListKnowledgeQuery, IEnumerable<KnowledgeEntry>>
{
    private readonly IKnowledgeRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListKnowledgeHandler"/> class.
    /// </summary>
    /// <param name="repository">The knowledge repository used for retrieval.</param>
    public ListKnowledgeHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the query by delegating to the repository's <see cref="IKnowledgeRepository.GetAllAsync"/> method.
    /// </summary>
    /// <param name="request">The list knowledge query.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A collection of all knowledge entries.</returns>
    public async Task<IEnumerable<KnowledgeEntry>> Handle(ListKnowledgeQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}
