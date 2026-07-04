using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Controlador que procesa el comando <see cref="DeleteKnowledgeTagCommand"/> para eliminar una etiqueta de conocimiento.
/// Implementa <see cref="IRequestHandler{TRequest,TResponse}"/> de MediatR y retorna un valor booleano
/// que indica si la operación de eliminación se completó con éxito.
/// </summary>
public class DeleteKnowledgeTagHandler : IRequestHandler<DeleteKnowledgeTagCommand, bool>
{
    private readonly IKnowledgeTagRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="DeleteKnowledgeTagHandler"/>.
    /// </summary>
    /// <param name="repository">Repositorio de etiquetas de conocimiento utilizado para acceder a los datos de persistencia.</param>
    public DeleteKnowledgeTagHandler(IKnowledgeTagRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja el comando <see cref="DeleteKnowledgeTagCommand"/> eliminando la etiqueta de conocimiento correspondiente al identificador especificado.
    /// </summary>
    /// <param name="request">Comando que contiene el identificador (<see cref="DeleteKnowledgeTagCommand.Id"/>) de la etiqueta a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación para propagar notificaciones de cancelación de la operación asincrónica.</param>
    /// <returns>
    /// <c>true</c> si la etiqueta existía y fue eliminada correctamente; <c>false</c> si no se encontró ninguna entidad
    /// con el identificador proporcionado.
    /// </returns>
    public async Task<bool> Handle(DeleteKnowledgeTagCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return false;

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
