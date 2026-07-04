using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

/// <summary>
/// Repositorio para la gestión de entidades <see cref="User"/>.
/// Proporciona operaciones básicas de persistencia: consulta por identificador,
/// guardado (creación/actualización) y eliminación lógica o física de usuarios.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Obtiene un usuario por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del usuario (<see cref="Guid"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// La tarea representa la operación asíncrona. El valor devuelto contiene la entidad <see cref="User"/>
    /// si se encuentra; en caso contrario, <c>null</c>.
    /// </returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda un usuario en el repositorio. Si el usuario ya existe, se actualiza;
    /// en caso contrario, se crea uno nuevo.
    /// </summary>
    /// <param name="user">Entidad <see cref="User"/> a guardar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de guardado.</returns>
    Task SaveAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un usuario del repositorio por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del usuario a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de eliminación.</returns>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
