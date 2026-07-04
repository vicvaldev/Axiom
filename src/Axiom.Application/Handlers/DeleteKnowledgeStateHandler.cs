using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Application.Handlers;

public class DeleteKnowledgeStateHandler : IRequestHandler<DeleteKnowledgeStateCommand, bool>
{
    private readonly IKnowledgeStateRepository _repository;

    public DeleteKnowledgeStateHandler(IKnowledgeStateRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteKnowledgeStateCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return false;

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
