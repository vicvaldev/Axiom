using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

/// <summary>
/// Repositorio de Entity Framework Core para la entidad <see cref="User"/>.
/// Implementa la interfaz <see cref="IUserRepository"/> para operaciones de persistencia
/// de usuarios en la base de datos SQL Server.
/// </summary>
public class EfUserRepository : IUserRepository
{
    private readonly AxiomDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio <see cref="EfUserRepository"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos de Axiom.</param>
    public EfUserRepository(AxiomDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene un usuario por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del usuario (<see cref="Guid"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// La entidad <see cref="User"/> si se encuentra; en caso contrario, <c>null</c>.
    /// </returns>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);
    }

    /// <summary>
    /// Guarda (crea o actualiza) un usuario en la base de datos.
    /// Si el usuario ya existe, se actualizan sus propiedades; si no, se agrega como nuevo.
    /// </summary>
    /// <param name="user">Entidad <see cref="User"/> a persistir.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de guardado.</returns>
    public async Task SaveAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Elimina un usuario de la base de datos por su identificador.
    /// Si el usuario no existe, la operación se completa sin errores.
    /// </summary>
    /// <param name="id">Identificador único del usuario a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de eliminación.</returns>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);

        if (entry is not null)
        {
            _context.Users.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
