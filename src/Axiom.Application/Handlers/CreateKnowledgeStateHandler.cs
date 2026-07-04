using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class CreateKnowledgeStateHandler : IRequestHandler<CreateKnowledgeStateCommand, KnowledgeState>
{
    private readonly IKnowledgeStateRepository _repository;

    public CreateKnowledgeStateHandler(IKnowledgeStateRepository repository)
    {
        _repository = repository;
    }

    public async Task<KnowledgeState> Handle(CreateKnowledgeStateCommand request, CancellationToken cancellationToken)
    {
        var entry = new KnowledgeState(request.Code, request.Name);
        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
