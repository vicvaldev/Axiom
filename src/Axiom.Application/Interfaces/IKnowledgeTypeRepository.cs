using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

/// <summary>
/// Repositorio para la gestión de entidades <see cref="KnowledgeType"/> (tipos de conocimiento).
/// Proporciona operaciones básicas de persistencia: consulta por identificador,
/// guardado (creación/actualización) y eliminación de tipos de conocimiento.
/// </summary>
public interface IKnowledgeTypeRepository
{
    /// <summary>
    /// Obtiene un tipo de conocimiento por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del tipo de conocimiento (clave primaria numérica <see cref="long"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// La tarea representa la operación asíncrona. El valor devuelto contiene la entidad <see cref="KnowledgeType"/>
    /// si se encuentra; en caso contrario, <c>null</c>.
    /// </returns>
    Task<KnowledgeType?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda un tipo de conocimiento en el repositorio. Si el tipo ya existe, se actualiza;
    /// en caso contrario, se crea uno nuevo.
    /// </summary>
    /// <param name="knowledgeType">Entidad <see cref="KnowledgeType"/> a guardar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de guardado.</returns>
    Task SaveAsync(KnowledgeType knowledgeType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un tipo de conocimiento del repositorio por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del tipo de conocimiento a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de eliminación.</returns>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
