using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Application.Handlers;

public class DeleteSystemHandler : IRequestHandler<DeleteSystemCommand, bool>
{
    private readonly ISystemRepository _repository;

    public DeleteSystemHandler(ISystemRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteSystemCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return false;

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
