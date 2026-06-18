using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Handles the <see cref="GetKnowledgeByIdQuery"/> by retrieving a single knowledge entry by its identifier.
/// </summary>
public class GetKnowledgeByIdHandler : IRequestHandler<GetKnowledgeByIdQuery, KnowledgeEntry?>
{
    private readonly IKnowledgeRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetKnowledgeByIdHandler"/> class.
    /// </summary>
    /// <param name="repository">The knowledge repository used for retrieval.</param>
    public GetKnowledgeByIdHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the query by delegating to the repository's <see cref="IKnowledgeRepository.GetByIdAsync"/> method.
    /// </summary>
    /// <param name="request">The query containing the entry identifier.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The matching knowledge entry, or <c>null</c> if not found.</returns>
    public async Task<KnowledgeEntry?> Handle(GetKnowledgeByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken);
    }
}
