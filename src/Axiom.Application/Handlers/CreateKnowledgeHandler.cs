using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Domain.ValueObjects;
using MediatR;

namespace Axiom.Application.Handlers;

public class CreateKnowledgeHandler : IRequestHandler<CreateKnowledgeCommand, KnowledgeEntry>
{
    private readonly IKnowledgeRepository _repository;

    public CreateKnowledgeHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    public async Task<KnowledgeEntry> Handle(CreateKnowledgeCommand request, CancellationToken cancellationToken)
    {
        var entry = new KnowledgeEntry(
            request.Title,
            request.Description,
            request.Content,
            new SystemName(request.System),
            request.Tags,
            request.Author,
            request.Type,
            request.Status);

        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
