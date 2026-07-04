using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, User?>
{
    private readonly IUserRepository _repository;

    public UpdateUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<User?> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return null;

        entry.Update(request.Email, request.Name);
        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
