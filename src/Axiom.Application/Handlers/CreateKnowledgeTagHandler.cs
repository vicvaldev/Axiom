using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class CreateKnowledgeTagHandler : IRequestHandler<CreateKnowledgeTagCommand, KnowledgeTag>
{
    private readonly IKnowledgeTagRepository _repository;

    public CreateKnowledgeTagHandler(IKnowledgeTagRepository repository)
    {
        _repository = repository;
    }

    public async Task<KnowledgeTag> Handle(CreateKnowledgeTagCommand request, CancellationToken cancellationToken)
    {
        var tag = new KnowledgeTag(request.TagName);
        await _repository.SaveAsync(tag, cancellationToken);
        return tag;
    }
}
