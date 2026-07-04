using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class CreateKnowledgeTypeHandler : IRequestHandler<CreateKnowledgeTypeCommand, KnowledgeType>
{
    private readonly IKnowledgeTypeRepository _repository;

    public CreateKnowledgeTypeHandler(IKnowledgeTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<KnowledgeType> Handle(CreateKnowledgeTypeCommand request, CancellationToken cancellationToken)
    {
        var entry = new KnowledgeType(request.Code, request.Name);
        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
