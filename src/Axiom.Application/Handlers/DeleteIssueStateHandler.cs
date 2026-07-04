using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Application.Handlers;

public class DeleteIssueStateHandler : IRequestHandler<DeleteIssueStateCommand, bool>
{
    private readonly IIssueStateRepository _repository;

    public DeleteIssueStateHandler(IIssueStateRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteIssueStateCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return false;

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
