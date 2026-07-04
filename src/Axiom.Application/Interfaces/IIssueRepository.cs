using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

/// <summary>
/// Repositorio para la gestión de entidades <see cref="Issue"/> (incidencias).
/// Proporciona operaciones de persistencia: consulta por identificador, listado completo,
/// búsqueda por número EAI, guardado (creación/actualización) y eliminación de incidencias.
/// </summary>
public interface IIssueRepository
{
    /// <summary>
    /// Guarda una incidencia en el repositorio. Si la incidencia ya existe, se actualiza;
    /// en caso contrario, se crea una nueva.
    /// </summary>
    /// <param name="issue">Entidad <see cref="Issue"/> a guardar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de guardado.</returns>
    Task SaveAsync(Issue issue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una incidencia por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único de la incidencia (<see cref="Guid"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// La tarea representa la operación asíncrona. El valor devuelto contiene la entidad <see cref="Issue"/>
    /// si se encuentra; en caso contrario, <c>null</c>.
    /// </returns>
    Task<Issue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las incidencias registradas en el repositorio.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona. El valor devuelto contiene una colección
    /// enumerable con todas las entidades <see cref="Issue"/>.
    /// </returns>
    Task<IEnumerable<Issue>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las incidencias asociadas a un sistema identificado por su número EAI.
    /// </summary>
    /// <param name="eai">Número EAI (código identificador del sistema) para filtrar las incidencias.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona. El valor devuelto contiene una colección
    /// enumerable con las entidades <see cref="Issue"/> que pertenecen al sistema con el EAI especificado.
    /// </returns>
    Task<IEnumerable<Issue>> GetByEaiAsync(string eai, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una incidencia del repositorio por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único de la incidencia a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de eliminación.</returns>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
