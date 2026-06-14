using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Domain.ValueObjects;
using MediatR;

namespace Axiom.Application.Handlers;

public class CreateCaseHandler : IRequestHandler<CreateCaseCommand, CaseRecord>
{
    private readonly ICaseRepository _repository;

    public CreateCaseHandler(ICaseRepository repository)
    {
        _repository = repository;
    }

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
