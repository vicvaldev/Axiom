using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Application.Handlers;

public class DeleteKnowledgeTypeHandler : IRequestHandler<DeleteKnowledgeTypeCommand, bool>
{
    private readonly IKnowledgeTypeRepository _repository;

    public DeleteKnowledgeTypeHandler(IKnowledgeTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteKnowledgeTypeCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return false;

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
