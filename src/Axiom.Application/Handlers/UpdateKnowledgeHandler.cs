using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;
using Axiom.Domain.Entities;

namespace Axiom.Application.Handlers;

public class UpdateKnowledgeHandler : IRequestHandler<UpdateKnowledgeCommand, Knowledge?>
{
    private readonly IKnowledgeRepository _repository;
    private readonly ITagRepository _tagRepository;

    public UpdateKnowledgeHandler(IKnowledgeRepository repository, ITagRepository tagRepository)
    {
        _repository = repository;
        _tagRepository = tagRepository;
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

        entry.KnowledgeKnowledgeTags.Clear();
        if (request.Tags?.Count > 0)
        {
            foreach (var tagName in request.Tags)
            {
                var tag = await _tagRepository.FindOrCreateAsync(tagName, cancellationToken);
                entry.KnowledgeKnowledgeTags.Add(
                    new KnowledgeKnowledgeTag(entry.KnowledgeId, tag.KnowledgeTagId));
            }
        }

        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
