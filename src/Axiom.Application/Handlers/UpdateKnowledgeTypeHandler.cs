using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class UpdateKnowledgeTypeHandler : IRequestHandler<UpdateKnowledgeTypeCommand, KnowledgeType?>
{
    private readonly IKnowledgeTypeRepository _repository;

    public UpdateKnowledgeTypeHandler(IKnowledgeTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<KnowledgeType?> Handle(UpdateKnowledgeTypeCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return null;

        entry.Update(request.Code, request.Name);
        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
