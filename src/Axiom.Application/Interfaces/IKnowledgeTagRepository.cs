using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

/// <summary>
/// Repositorio para la gestión de entidades <see cref="KnowledgeTag"/> (etiquetas de conocimiento).
/// Proporciona operaciones básicas de persistencia: consulta por identificador,
/// listado completo, guardado (creación/actualización) y eliminación de etiquetas.
/// </summary>
public interface IKnowledgeTagRepository
{
    /// <summary>
    /// Obtiene una etiqueta de conocimiento por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único de la etiqueta (clave primaria numérica <see cref="long"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// La tarea representa la operación asíncrona. El valor devuelto contiene la entidad <see cref="KnowledgeTag"/>
    /// si se encuentra; en caso contrario, <c>null</c>.
    /// </returns>
    Task<KnowledgeTag?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda una etiqueta de conocimiento en el repositorio. Si la etiqueta ya existe, se actualiza;
    /// en caso contrario, se crea una nueva.
    /// </summary>
    /// <param name="tag">Entidad <see cref="KnowledgeTag"/> a guardar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de guardado.</returns>
    Task SaveAsync(KnowledgeTag tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las etiquetas de conocimiento disponibles en el repositorio.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona. El valor devuelto contiene una lista
    /// con todas las entidades <see cref="KnowledgeTag"/>.
    /// </returns>
    Task<List<KnowledgeTag>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una etiqueta de conocimiento del repositorio por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único de la etiqueta a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de eliminación.</returns>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
