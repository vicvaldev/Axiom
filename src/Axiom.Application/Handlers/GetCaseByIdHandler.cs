using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Handles the <see cref="GetCaseByIdQuery"/> by retrieving a single case record by its identifier.
/// </summary>
public class GetCaseByIdHandler : IRequestHandler<GetCaseByIdQuery, CaseRecord?>
{
    private readonly ICaseRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCaseByIdHandler"/> class.
    /// </summary>
    /// <param name="repository">The case repository used for retrieval.</param>
    public GetCaseByIdHandler(ICaseRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the query by delegating to the repository's <see cref="ICaseRepository.GetByIdAsync"/> method.
    /// </summary>
    /// <param name="request">The query containing the case record identifier.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The matching case record, or <c>null</c> if not found.</returns>
    public async Task<CaseRecord?> Handle(GetCaseByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken);
    }
}
