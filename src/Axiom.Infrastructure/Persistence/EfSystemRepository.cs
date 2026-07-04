using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

/// <summary>
/// Repositorio de Entity Framework Core para la entidad <see cref="AxiomSystem"/>.
/// Implementa la interfaz <see cref="ISystemRepository"/> para operaciones de persistencia
/// de sistemas en la base de datos SQL Server.
/// </summary>
public class EfSystemRepository : ISystemRepository
{
    private readonly AxiomDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio <see cref="EfSystemRepository"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos de Axiom.</param>
    public EfSystemRepository(AxiomDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene un sistema por su identificador numérico.
    /// </summary>
    /// <param name="id">Identificador único del sistema (<see cref="long"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// La entidad <see cref="AxiomSystem"/> si se encuentra; en caso contrario, <c>null</c>.
    /// </returns>
    public async Task<AxiomSystem?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Systems
            .FirstOrDefaultAsync(s => s.SystemId == id, cancellationToken);
    }

    /// <summary>
    /// Guarda (crea o actualiza) un sistema en la base de datos.
    /// Si el sistema ya existe, se actualizan sus propiedades; si no, se agrega como nuevo.
    /// </summary>
    /// <param name="system">Entidad <see cref="AxiomSystem"/> a persistir.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de guardado.</returns>
    public async Task SaveAsync(AxiomSystem system, CancellationToken cancellationToken = default)
    {
        _context.Systems.Update(system);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Elimina un sistema de la base de datos por su identificador.
    /// Si el sistema no existe, la operación se completa sin errores.
    /// </summary>
    /// <param name="id">Identificador único del sistema a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de eliminación.</returns>
    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Systems
            .FirstOrDefaultAsync(s => s.SystemId == id, cancellationToken);

        if (entry is not null)
        {
            _context.Systems.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
