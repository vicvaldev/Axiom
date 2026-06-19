using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class CreateKnowledgeHandler : IRequestHandler<CreateKnowledgeCommand, Knowledge>
{
    private readonly IKnowledgeRepository _repository;
    private readonly ITagRepository _tagRepository;

    public CreateKnowledgeHandler(IKnowledgeRepository repository, ITagRepository tagRepository)
    {
        _repository = repository;
        _tagRepository = tagRepository;
    }

    public async Task<Knowledge> Handle(CreateKnowledgeCommand request, CancellationToken cancellationToken)
    {
        var entry = new Knowledge(
            request.Title,
            request.Summary,
            request.Content,
            request.SystemId,
            request.CreatedByUserId,
            request.KnowledgeTypeId,
            request.KnowledgeStateId,
            request.IssueId);

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
