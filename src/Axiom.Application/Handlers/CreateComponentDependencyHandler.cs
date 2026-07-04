using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class CreateComponentDependencyHandler : IRequestHandler<CreateComponentDependencyCommand, ComponentDependency>
{
    private readonly IComponentDependencyRepository _repository;
    private readonly ITechnicalComponentRepository _componentRepository;

    public CreateComponentDependencyHandler(
        IComponentDependencyRepository repository,
        ITechnicalComponentRepository componentRepository)
    {
        _repository = repository;
        _componentRepository = componentRepository;
    }

    public async Task<ComponentDependency> Handle(CreateComponentDependencyCommand request, CancellationToken cancellationToken)
    {
        var source = await _componentRepository.GetByIdAsync(request.SourceComponentId, cancellationToken);
        if (source is null)
            throw new ArgumentException($"Source component not found: {request.SourceComponentId}", nameof(request.SourceComponentId));

        var target = await _componentRepository.GetByIdAsync(request.TargetComponentId, cancellationToken);
        if (target is null)
            throw new ArgumentException($"Target component not found: {request.TargetComponentId}", nameof(request.TargetComponentId));

        var entry = new ComponentDependency(
            request.SourceComponentId,
            request.TargetComponentId,
            request.DependencyType,
            request.Criticality,
            request.Status,
            request.Description);

        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
