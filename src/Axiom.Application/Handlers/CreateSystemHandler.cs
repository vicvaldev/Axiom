using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class CreateSystemHandler : IRequestHandler<CreateSystemCommand, AxiomSystem>
{
    private readonly ISystemRepository _repository;

    public CreateSystemHandler(ISystemRepository repository)
    {
        _repository = repository;
    }

    public async Task<AxiomSystem> Handle(CreateSystemCommand request, CancellationToken cancellationToken)
    {
        var system = new AxiomSystem(request.EAI, request.Name, request.OwnerUserId);
        await _repository.SaveAsync(system, cancellationToken);
        return system;
    }
}
