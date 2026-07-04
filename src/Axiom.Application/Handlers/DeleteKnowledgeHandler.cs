using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Controlador que procesa el comando <see cref="DeleteKnowledgeCommand"/> para eliminar un conocimiento del sistema.
/// Implementa <see cref="IRequestHandler{TRequest,TResponse}"/> de MediatR y retorna un valor booleano
/// que indica si la operación de eliminación se completó con éxito.
/// </summary>
public class DeleteKnowledgeHandler : IRequestHandler<DeleteKnowledgeCommand, bool>
{
    private readonly IKnowledgeRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="DeleteKnowledgeHandler"/>.
    /// </summary>
    /// <param name="repository">Repositorio de conocimientos utilizado para acceder a los datos de persistencia.</param>
    public DeleteKnowledgeHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja el comando <see cref="DeleteKnowledgeCommand"/> eliminando el conocimiento correspondiente al identificador especificado.
    /// </summary>
    /// <param name="request">Comando que contiene el identificador (<see cref="DeleteKnowledgeCommand.Id"/>) del conocimiento a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación para propagar notificaciones de cancelación de la operación asincrónica.</param>
    /// <returns>
    /// <c>true</c> si el conocimiento existía y fue eliminado correctamente; <c>false</c> si no se encontró ninguna entidad
    /// con el identificador proporcionado.
    /// </returns>
    public async Task<bool> Handle(DeleteKnowledgeCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return false;

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
