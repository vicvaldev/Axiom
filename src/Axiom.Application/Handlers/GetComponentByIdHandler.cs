using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using MediatR;

namespace Axiom.Application.Handlers;

public class GetComponentByIdHandler : IRequestHandler<GetComponentByIdQuery, TechnicalComponentDto?>
{
    private readonly ITechnicalComponentRepository _repository;

    public GetComponentByIdHandler(ITechnicalComponentRepository repository)
    {
        _repository = repository;
    }

    public async Task<TechnicalComponentDto?> Handle(GetComponentByIdQuery request, CancellationToken cancellationToken)
    {
        var component = await _repository.GetByIdAsync(request.ComponentId, cancellationToken);

        if (component is null)
            return null;

        return new TechnicalComponentDto
        {
            ComponentId = component.ComponentId,
            Name = component.Name,
            TechnicalName = component.TechnicalName,
            ComponentType = component.ComponentType.ToString(),
            Description = component.Description,
            Environment = component.Environment.ToString(),
            Criticality = component.Criticality.ToString(),
            SystemId = component.SystemId,
            SystemName = component.System?.Name ?? string.Empty,
            CreatedAt = component.CreatedAt,
            UpdatedAt = component.UpdatedAt
        };
    }
}
