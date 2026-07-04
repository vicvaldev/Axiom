using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class CreateSystemComponentHandler : IRequestHandler<CreateSystemComponentCommand, SystemComponent>
{
    private readonly ISystemComponentRepository _repository;
    private readonly ITechnicalComponentRepository _componentRepository;

    public CreateSystemComponentHandler(
        ISystemComponentRepository repository,
        ITechnicalComponentRepository componentRepository)
    {
        _repository = repository;
        _componentRepository = componentRepository;
    }

    public async Task<SystemComponent> Handle(CreateSystemComponentCommand request, CancellationToken cancellationToken)
    {
        var component = await _componentRepository.GetByIdAsync(request.ComponentId, cancellationToken);
        if (component is null)
            throw new ArgumentException($"Component not found: {request.ComponentId}", nameof(request.ComponentId));

        var entry = new SystemComponent(
            request.SystemId,
            request.ComponentId,
            request.IsOwner,
            request.RoleDescription);

        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
