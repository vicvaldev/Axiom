using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class CreateDependencyTraceEventHandler : IRequestHandler<CreateDependencyTraceEventCommand, DependencyTraceEvent>
{
    private readonly IDependencyTraceEventRepository _repository;

    public CreateDependencyTraceEventHandler(IDependencyTraceEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<DependencyTraceEvent> Handle(CreateDependencyTraceEventCommand request, CancellationToken cancellationToken)
    {
        var entry = new DependencyTraceEvent(
            request.DependencyId,
            request.EventType,
            request.Description,
            request.RelatedIssueId,
            request.RelatedKnowledgeId,
            request.RelatedRitmNumber,
            request.RelatedChangeNumber,
            request.CreatedByUserId);

        await _repository.AddAsync(entry, cancellationToken);
        return entry;
    }
}
