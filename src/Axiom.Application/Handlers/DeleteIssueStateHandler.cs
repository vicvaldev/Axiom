using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Controlador que procesa el comando <see cref="DeleteIssueStateCommand"/> para eliminar un estado de incidencia.
/// Implementa <see cref="IRequestHandler{TRequest,TResponse}"/> de MediatR y retorna un valor booleano
/// que indica si la operación de eliminación se completó con éxito.
/// </summary>
public class DeleteIssueStateHandler : IRequestHandler<DeleteIssueStateCommand, bool>
{
    private readonly IIssueStateRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="DeleteIssueStateHandler"/>.
    /// </summary>
    /// <param name="repository">Repositorio de estados de incidencia utilizado para acceder a los datos de persistencia.</param>
    public DeleteIssueStateHandler(IIssueStateRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja el comando <see cref="DeleteIssueStateCommand"/> eliminando el estado de incidencia correspondiente al identificador especificado.
    /// </summary>
    /// <param name="request">Comando que contiene el identificador (<see cref="DeleteIssueStateCommand.Id"/>) del estado de incidencia a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación para propagar notificaciones de cancelación de la operación asincrónica.</param>
    /// <returns>
    /// <c>true</c> si el estado de incidencia existía y fue eliminado correctamente; <c>false</c> si no se encontró ninguna entidad
    /// con el identificador proporcionado.
    /// </returns>
    public async Task<bool> Handle(DeleteIssueStateCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return false;

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
