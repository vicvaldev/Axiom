using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Controlador que procesa el comando <see cref="DeleteUserCommand"/> para eliminar un usuario del sistema.
/// Implementa <see cref="IRequestHandler{TRequest,TResponse}"/> de MediatR y retorna un valor booleano
/// que indica si la operación de eliminación se completó con éxito.
/// </summary>
public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IUserRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="DeleteUserHandler"/>.
    /// </summary>
    /// <param name="repository">Repositorio de usuarios utilizado para acceder a los datos de persistencia.</param>
    public DeleteUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja el comando <see cref="DeleteUserCommand"/> eliminando el usuario correspondiente al identificador especificado.
    /// </summary>
    /// <param name="request">Comando que contiene el identificador (<see cref="DeleteUserCommand.Id"/>) del usuario a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación para propagar notificaciones de cancelación de la operación asincrónica.</param>
    /// <returns>
    /// <c>true</c> si el usuario existía y fue eliminado correctamente; <c>false</c> si no se encontró ninguna entidad
    /// con el identificador proporcionado.
    /// </returns>
    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return false;

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
