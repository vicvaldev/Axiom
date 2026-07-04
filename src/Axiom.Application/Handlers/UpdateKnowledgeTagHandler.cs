using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class UpdateKnowledgeTagHandler : IRequestHandler<UpdateKnowledgeTagCommand, KnowledgeTag?>
{
    private readonly IKnowledgeTagRepository _repository;

    public UpdateKnowledgeTagHandler(IKnowledgeTagRepository repository)
    {
        _repository = repository;
    }

    public async Task<KnowledgeTag?> Handle(UpdateKnowledgeTagCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return null;

        entry.Update(request.TagName);
        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
