using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class UpdateKnowledgeStateHandler : IRequestHandler<UpdateKnowledgeStateCommand, KnowledgeState?>
{
    private readonly IKnowledgeStateRepository _repository;

    public UpdateKnowledgeStateHandler(IKnowledgeStateRepository repository)
    {
        _repository = repository;
    }

    public async Task<KnowledgeState?> Handle(UpdateKnowledgeStateCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return null;

        entry.Update(request.Code, request.Name);
        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
