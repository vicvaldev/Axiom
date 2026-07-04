using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
///     Manejador del comando <see cref="UpdateUserCommand" />.
///     Actualiza los datos de un usuario existente en el repositorio.
///     Si el usuario no se encuentra, retorna <c>null</c> sin realizar cambios.
/// </summary>
public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, User?>
{
    private readonly IUserRepository _repository;

    /// <summary>
    ///     Inicializa una nueva instancia de la clase <see cref="UpdateUserHandler" />.
    /// </summary>
    /// <param name="repository">Repositorio de usuarios (<see cref="IUserRepository" />).</param>
    public UpdateUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    ///     Procesa el comando de actualización de usuario.
    ///     Busca la entidad por identificador, aplica los cambios con <see cref="User.Update" />
    ///     y persiste la entidad modificada en el repositorio.
    /// </summary>
    /// <param name="request">Comando que contiene el identificador del usuario y los nuevos valores
    /// (<see cref="UpdateUserCommand.Id" />, <see cref="UpdateUserCommand.Email" />,
    /// <see cref="UpdateUserCommand.Name" />).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="User" /> actualizada, o <c>null</c> si no se encuentra el usuario.</returns>
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
