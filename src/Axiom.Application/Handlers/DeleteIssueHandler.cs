using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Controlador que procesa el comando <see cref="DeleteIssueCommand"/> para eliminar un incidente o requerimiento.
/// Implementa <see cref="IRequestHandler{TRequest,TResponse}"/> de MediatR y retorna un valor booleano
/// que indica si la operación de eliminación se completó con éxito.
/// </summary>
public class DeleteIssueHandler : IRequestHandler<DeleteIssueCommand, bool>
{
    private readonly IIssueRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="DeleteIssueHandler"/>.
    /// </summary>
    /// <param name="repository">Repositorio de incidencias utilizado para acceder a los datos de persistencia.</param>
    public DeleteIssueHandler(IIssueRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja el comando <see cref="DeleteIssueCommand"/> eliminando la incidencia correspondiente al identificador especificado.
    /// </summary>
    /// <param name="request">Comando que contiene el identificador (<see cref="DeleteIssueCommand.Id"/>) de la incidencia a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación para propagar notificaciones de cancelación de la operación asincrónica.</param>
    /// <returns>
    /// <c>true</c> si la incidencia existía y fue eliminada correctamente; <c>false</c> si no se encontró ninguna entidad
    /// con el identificador proporcionado.
    /// </returns>
    public async Task<bool> Handle(DeleteIssueCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return false;

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
