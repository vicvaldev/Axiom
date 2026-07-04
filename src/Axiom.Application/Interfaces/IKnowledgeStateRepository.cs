using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

/// <summary>
/// Repositorio para la gestión de entidades <see cref="KnowledgeState"/> (estados de conocimiento).
/// Proporciona operaciones básicas de persistencia: consulta por identificador,
/// guardado (creación/actualización) y eliminación de estados de conocimiento.
/// </summary>
public interface IKnowledgeStateRepository
{
    /// <summary>
    /// Obtiene un estado de conocimiento por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del estado (clave primaria numérica <see cref="int"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// La tarea representa la operación asíncrona. El valor devuelto contiene la entidad <see cref="KnowledgeState"/>
    /// si se encuentra; en caso contrario, <c>null</c>.
    /// </returns>
    Task<KnowledgeState?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda un estado de conocimiento en el repositorio. Si el estado ya existe, se actualiza;
    /// en caso contrario, se crea uno nuevo.
    /// </summary>
    /// <param name="knowledgeState">Entidad <see cref="KnowledgeState"/> a guardar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de guardado.</returns>
    Task SaveAsync(KnowledgeState knowledgeState, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un estado de conocimiento del repositorio por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del estado de conocimiento a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de eliminación.</returns>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
