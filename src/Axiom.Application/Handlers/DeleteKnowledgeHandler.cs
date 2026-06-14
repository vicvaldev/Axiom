using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Application.Handlers;

public class DeleteKnowledgeHandler : IRequestHandler<DeleteKnowledgeCommand, bool>
{
    private readonly IKnowledgeRepository _repository;

    public DeleteKnowledgeHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteKnowledgeCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return false;

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
