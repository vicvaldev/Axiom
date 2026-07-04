using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Application.Handlers;

public class DeleteIssueHandler : IRequestHandler<DeleteIssueCommand, bool>
{
    private readonly IIssueRepository _repository;

    public DeleteIssueHandler(IIssueRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteIssueCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return false;

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
