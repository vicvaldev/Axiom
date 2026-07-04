using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Manejador del comando <see cref="CreateUserCommand"/>.
/// Crea una nueva entidad <see cref="User"/> a partir de los datos proporcionados
/// en el comando y la persiste a través del repositorio <see cref="IUserRepository"/>.
/// </summary>
public class CreateUserHandler : IRequestHandler<CreateUserCommand, User>
{
    private readonly IUserRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CreateUserHandler"/>.
    /// </summary>
    /// <param name="repository">Repositorio de usuarios utilizado para persistir la entidad <see cref="User"/>.</param>
    public CreateUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja la creación de un nuevo usuario.
    /// Construye una entidad <see cref="User"/> con el correo electrónico y el nombre
    /// especificados en el comando, la guarda en el repositorio y la retorna.
    /// </summary>
    /// <param name="request">Comando que contiene los datos del usuario a crear (<see cref="CreateUserCommand"/>).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="User"/> recién creada y persistida.</returns>
    public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User(request.Email, request.Name);
        await _repository.SaveAsync(user, cancellationToken);
        return user;
    }
}
