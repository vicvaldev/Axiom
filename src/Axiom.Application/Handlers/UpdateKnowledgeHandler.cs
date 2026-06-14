using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Domain.ValueObjects;
using MediatR;

namespace Axiom.Application.Handlers;

public class UpdateKnowledgeHandler : IRequestHandler<UpdateKnowledgeCommand, KnowledgeEntry?>
{
    private readonly IKnowledgeRepository _repository;

    public UpdateKnowledgeHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    public async Task<KnowledgeEntry?> Handle(UpdateKnowledgeCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return null;

        entry.Update(
            request.Title,
            request.Description,
            request.Content,
            new SystemName(request.System),
            request.Tags,
            request.Type,
            request.Status);

        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
