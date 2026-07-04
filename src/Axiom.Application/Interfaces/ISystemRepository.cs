using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

/// <summary>
/// Repositorio para la gestión de entidades <see cref="AxiomSystem"/>.
/// Proporciona operaciones básicas de persistencia: consulta por identificador,
/// guardado (creación/actualización) y eliminación de sistemas.
/// </summary>
public interface ISystemRepository
{
    /// <summary>
    /// Obtiene un sistema por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del sistema (clave primaria numérica <see cref="long"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// La tarea representa la operación asíncrona. El valor devuelto contiene la entidad <see cref="AxiomSystem"/>
    /// si se encuentra; en caso contrario, <c>null</c>.
    /// </returns>
    Task<AxiomSystem?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda un sistema en el repositorio. Si el sistema ya existe, se actualiza;
    /// en caso contrario, se crea uno nuevo.
    /// </summary>
    /// <param name="system">Entidad <see cref="AxiomSystem"/> a guardar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de guardado.</returns>
    Task SaveAsync(AxiomSystem system, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un sistema del repositorio por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del sistema a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de eliminación.</returns>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
