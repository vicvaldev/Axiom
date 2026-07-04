using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Controlador que procesa el comando <see cref="DeleteKnowledgeTypeCommand"/> para eliminar un tipo de conocimiento.
/// Implementa <see cref="IRequestHandler{TRequest,TResponse}"/> de MediatR y retorna un valor booleano
/// que indica si la operación de eliminación se completó con éxito.
/// </summary>
public class DeleteKnowledgeTypeHandler : IRequestHandler<DeleteKnowledgeTypeCommand, bool>
{
    private readonly IKnowledgeTypeRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="DeleteKnowledgeTypeHandler"/>.
    /// </summary>
    /// <param name="repository">Repositorio de tipos de conocimiento utilizado para acceder a los datos de persistencia.</param>
    public DeleteKnowledgeTypeHandler(IKnowledgeTypeRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja el comando <see cref="DeleteKnowledgeTypeCommand"/> eliminando el tipo de conocimiento correspondiente al identificador especificado.
    /// </summary>
    /// <param name="request">Comando que contiene el identificador (<see cref="DeleteKnowledgeTypeCommand.Id"/>) del tipo de conocimiento a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación para propagar notificaciones de cancelación de la operación asincrónica.</param>
    /// <returns>
    /// <c>true</c> si el tipo de conocimiento existía y fue eliminado correctamente; <c>false</c> si no se encontró ninguna entidad
    /// con el identificador proporcionado.
    /// </returns>
    public async Task<bool> Handle(DeleteKnowledgeTypeCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return false;

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
