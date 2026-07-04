using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class CreateTechnicalComponentHandler : IRequestHandler<CreateTechnicalComponentCommand, TechnicalComponent>
{
    private readonly ITechnicalComponentRepository _repository;

    public CreateTechnicalComponentHandler(ITechnicalComponentRepository repository)
    {
        _repository = repository;
    }

    public async Task<TechnicalComponent> Handle(CreateTechnicalComponentCommand request, CancellationToken cancellationToken)
    {
        var entry = new TechnicalComponent(
            request.Name,
            request.TechnicalName,
            request.ComponentType,
            request.Environment,
            request.Criticality,
            request.SystemId,
            request.Description);

        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
