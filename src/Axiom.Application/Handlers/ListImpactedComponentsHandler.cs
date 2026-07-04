using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using MediatR;

namespace Axiom.Application.Handlers;

public class ListImpactedComponentsHandler : IRequestHandler<ListImpactedComponentsQuery, IEnumerable<ComponentDependencyDto>>
{
    private readonly IComponentDependencyRepository _repository;

    public ListImpactedComponentsHandler(IComponentDependencyRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ComponentDependencyDto>> Handle(ListImpactedComponentsQuery request, CancellationToken cancellationToken)
    {
        var entries = await _repository.GetImpactedByComponentAsync(request.ComponentId, cancellationToken);

        return entries.Select(d => new ComponentDependencyDto
        {
            DependencyId = d.DependencyId,
            SourceComponentId = d.SourceComponentId,
            SourceComponentName = d.Source?.Name ?? string.Empty,
            SourceTechnicalName = d.Source?.TechnicalName ?? string.Empty,
            TargetComponentId = d.TargetComponentId,
            TargetComponentName = d.Target?.Name ?? string.Empty,
            TargetTechnicalName = d.Target?.TechnicalName ?? string.Empty,
            DependencyType = d.DependencyType.ToString(),
            Description = d.Description,
            Criticality = d.Criticality.ToString(),
            Status = d.Status.ToString(),
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        });
    }
}
