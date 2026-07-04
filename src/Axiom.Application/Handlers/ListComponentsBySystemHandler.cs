using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using MediatR;

namespace Axiom.Application.Handlers;

public class ListComponentsBySystemHandler : IRequestHandler<ListComponentsBySystemQuery, IEnumerable<TechnicalComponentDto>>
{
    private readonly ITechnicalComponentRepository _repository;

    public ListComponentsBySystemHandler(ITechnicalComponentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TechnicalComponentDto>> Handle(ListComponentsBySystemQuery request, CancellationToken cancellationToken)
    {
        var entries = await _repository.GetBySystemIdAsync(request.SystemId, cancellationToken);

        return entries.Select(c => new TechnicalComponentDto
        {
            ComponentId = c.ComponentId,
            Name = c.Name,
            TechnicalName = c.TechnicalName,
            ComponentType = c.ComponentType.ToString(),
            Description = c.Description,
            Environment = c.Environment.ToString(),
            Criticality = c.Criticality.ToString(),
            SystemId = c.SystemId,
            SystemName = c.System?.Name ?? string.Empty,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        });
    }
}
