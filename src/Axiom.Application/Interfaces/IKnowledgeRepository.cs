using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

/// <summary>
/// Repositorio para la gestión de entidades <see cref="Knowledge"/> (artículos de conocimiento).
/// Proporciona operaciones de persistencia: consulta por identificador, listado completo,
/// búsqueda textual, guardado (creación/actualización) y eliminación de entradas de conocimiento.
/// </summary>
public interface IKnowledgeRepository
{
    /// <summary>
    /// Guarda una entrada de conocimiento en el repositorio. Si la entrada ya existe, se actualiza;
    /// en caso contrario, se crea una nueva.
    /// </summary>
    /// <param name="entry">Entidad <see cref="Knowledge"/> a guardar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de guardado.</returns>
    Task SaveAsync(Knowledge entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una entrada de conocimiento por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único de la entrada de conocimiento (<see cref="Guid"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// La tarea representa la operación asíncrona. El valor devuelto contiene la entidad <see cref="Knowledge"/>
    /// si se encuentra; en caso contrario, <c>null</c>.
    /// </returns>
    Task<Knowledge?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza una búsqueda textual sobre las entradas de conocimiento.
    /// El criterio de búsqueda se aplica sobre campos relevantes como título, contenido y resumen.
    /// </summary>
    /// <param name="query">Texto de búsqueda para localizar entradas de conocimiento.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona. El valor devuelto contiene una colección
    /// enumerable con las entidades <see cref="Knowledge"/> que coinciden con el criterio de búsqueda.
    /// </returns>
    Task<IEnumerable<Knowledge>> SearchAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las entradas de conocimiento registradas en el repositorio.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona. El valor devuelto contiene una colección
    /// enumerable con todas las entidades <see cref="Knowledge"/>.
    /// </returns>
    Task<IEnumerable<Knowledge>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una entrada de conocimiento del repositorio por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único de la entrada de conocimiento a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de eliminación.</returns>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
