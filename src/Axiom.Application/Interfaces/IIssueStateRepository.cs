using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

/// <summary>
/// Repositorio para la gestión de entidades <see cref="IssueState"/> (estados de incidencias).
/// Proporciona operaciones básicas de persistencia: consulta por identificador,
/// guardado (creación/actualización) y eliminación de estados de incidencia.
/// </summary>
public interface IIssueStateRepository
{
    /// <summary>
    /// Obtiene un estado de incidencia por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del estado (clave primaria numérica <see cref="int"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// La tarea representa la operación asíncrona. El valor devuelto contiene la entidad <see cref="IssueState"/>
    /// si se encuentra; en caso contrario, <c>null</c>.
    /// </returns>
    Task<IssueState?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda un estado de incidencia en el repositorio. Si el estado ya existe, se actualiza;
    /// en caso contrario, se crea uno nuevo.
    /// </summary>
    /// <param name="issueState">Entidad <see cref="IssueState"/> a guardar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de guardado.</returns>
    Task SaveAsync(IssueState issueState, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un estado de incidencia del repositorio por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del estado de incidencia a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de eliminación.</returns>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
