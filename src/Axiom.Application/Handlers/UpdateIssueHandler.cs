using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;
using Axiom.Domain.Entities;

namespace Axiom.Application.Handlers;

public class UpdateIssueHandler : IRequestHandler<UpdateIssueCommand, Issue?>
{
    private readonly IIssueRepository _repository;

    public UpdateIssueHandler(IIssueRepository repository)
    {
        _repository = repository;
    }

    public async Task<Issue?> Handle(UpdateIssueCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return null;

        entry.Update(
            request.Summary,
            request.Problem,
            request.Analysis,
            request.Resolution,
            request.SystemId,
            request.StateId,
            request.RitmNumber,
            request.IncidentNumber);

        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
