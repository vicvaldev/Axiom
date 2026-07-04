using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Application.Handlers;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IUserRepository _repository;

    public DeleteUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return false;

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
