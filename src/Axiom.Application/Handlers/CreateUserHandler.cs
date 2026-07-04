using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, User>
{
    private readonly IUserRepository _repository;

    public CreateUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User(request.Email, request.Name);
        await _repository.SaveAsync(user, cancellationToken);
        return user;
    }
}
