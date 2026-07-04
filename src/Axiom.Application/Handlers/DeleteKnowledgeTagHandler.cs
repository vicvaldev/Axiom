using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Application.Handlers;

public class DeleteKnowledgeTagHandler : IRequestHandler<DeleteKnowledgeTagCommand, bool>
{
    private readonly IKnowledgeTagRepository _repository;

    public DeleteKnowledgeTagHandler(IKnowledgeTagRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteKnowledgeTagCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return false;

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
