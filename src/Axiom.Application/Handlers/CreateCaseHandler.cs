using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Domain.ValueObjects;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Handles the <see cref="CreateCaseCommand"/> by creating a new <see cref="CaseRecord"/> and persisting it.
/// </summary>
public class CreateCaseHandler : IRequestHandler<CreateCaseCommand, CaseRecord>
{
    private readonly ICaseRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCaseHandler"/> class.
    /// </summary>
    /// <param name="repository">The case repository used for persistence.</param>
    public CreateCaseHandler(ICaseRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the command by constructing a <see cref="CaseRecord"/> from the request data and saving it.
    /// </summary>
    /// <param name="request">The create case command.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The newly created <see cref="CaseRecord"/>.</returns>
    public async Task<CaseRecord> Handle(CreateCaseCommand request, CancellationToken cancellationToken)
    {
        var record = new CaseRecord(
            new SystemName(request.System),
            request.Problem,
            request.Analysis,
            request.Resolution,
            request.LessonsLearned,
            request.RitmId is not null ? new RitmId(request.RitmId) : null,
            request.ChangeId);

        await _repository.SaveAsync(record, cancellationToken);
        return record;
    }
}
