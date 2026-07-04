using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Controlador que procesa el comando <see cref="DeleteSystemCommand"/> para eliminar un sistema del dominio.
/// Implementa <see cref="IRequestHandler{TRequest,TResponse}"/> de MediatR y retorna un valor booleano
/// que indica si la operación de eliminación se completó con éxito.
/// </summary>
public class DeleteSystemHandler : IRequestHandler<DeleteSystemCommand, bool>
{
    private readonly ISystemRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="DeleteSystemHandler"/>.
    /// </summary>
    /// <param name="repository">Repositorio de sistemas utilizado para acceder a los datos de persistencia.</param>
    public DeleteSystemHandler(ISystemRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja el comando <see cref="DeleteSystemCommand"/> eliminando el sistema correspondiente al identificador especificado.
    /// </summary>
    /// <param name="request">Comando que contiene el identificador (<see cref="DeleteSystemCommand.Id"/>) del sistema a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación para propagar notificaciones de cancelación de la operación asincrónica.</param>
    /// <returns>
    /// <c>true</c> si el sistema existía y fue eliminado correctamente; <c>false</c> si no se encontró ninguna entidad
    /// con el identificador proporcionado.
    /// </returns>
    public async Task<bool> Handle(DeleteSystemCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return false;

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
