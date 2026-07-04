using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class UpdateSystemHandler : IRequestHandler<UpdateSystemCommand, AxiomSystem?>
{
    private readonly ISystemRepository _repository;

    public UpdateSystemHandler(ISystemRepository repository)
    {
        _repository = repository;
    }

    public async Task<AxiomSystem?> Handle(UpdateSystemCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return null;

        entry.Update(request.EAI, request.Name, request.OwnerUserId);
        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
