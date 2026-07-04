using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class CreateIssueStateHandler : IRequestHandler<CreateIssueStateCommand, IssueState>
{
    private readonly IIssueStateRepository _repository;

    public CreateIssueStateHandler(IIssueStateRepository repository)
    {
        _repository = repository;
    }

    public async Task<IssueState> Handle(CreateIssueStateCommand request, CancellationToken cancellationToken)
    {
        var entry = new IssueState(request.Code, request.Name);
        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
