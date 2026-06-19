using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class CreateIssueHandler : IRequestHandler<CreateIssueCommand, Issue>
{
    private readonly IIssueRepository _repository;

    public CreateIssueHandler(IIssueRepository repository)
    {
        _repository = repository;
    }

    public async Task<Issue> Handle(CreateIssueCommand request, CancellationToken cancellationToken)
    {
        var issue = new Issue(
            request.Summary,
            request.SystemId,
            request.Problem,
            request.StateId,
            request.CreatedByUserId,
            request.Analysis,
            request.Resolution,
            request.RitmNumber,
            request.IncidentNumber);

        await _repository.SaveAsync(issue, cancellationToken);
        return issue;
    }
}
