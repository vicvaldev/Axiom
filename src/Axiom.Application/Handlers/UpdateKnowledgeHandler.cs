using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;
using Axiom.Domain.Entities;

namespace Axiom.Application.Handlers;

public class UpdateKnowledgeHandler : IRequestHandler<UpdateKnowledgeCommand, Knowledge?>
{
    private readonly IKnowledgeRepository _repository;

    public UpdateKnowledgeHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Knowledge?> Handle(UpdateKnowledgeCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return null;

        entry.Update(
            request.Title,
            request.Summary,
            request.Content,
            request.SystemId,
            request.KnowledgeTypeId,
            request.KnowledgeStateId,
            request.IssueId);

        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
