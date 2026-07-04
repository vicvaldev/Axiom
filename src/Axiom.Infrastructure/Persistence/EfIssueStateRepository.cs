using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

/// <summary>
/// Repositorio de Entity Framework Core para la entidad <see cref="IssueState"/>.
/// Implementa la interfaz <see cref="IIssueStateRepository"/> para operaciones de persistencia
/// de estados de incidencia en la base de datos SQL Server.
/// </summary>
public class EfIssueStateRepository : IIssueStateRepository
{
    private readonly AxiomDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio <see cref="EfIssueStateRepository"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos de Axiom.</param>
    public EfIssueStateRepository(AxiomDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene un estado de incidencia por su identificador numérico.
    /// </summary>
    /// <param name="id">Identificador único del estado de incidencia (<see cref="int"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// La entidad <see cref="IssueState"/> si se encuentra; en caso contrario, <c>null</c>.
    /// </returns>
    public async Task<IssueState?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.IssueStates
            .FirstOrDefaultAsync(s => s.StateId == id, cancellationToken);
    }

    /// <summary>
    /// Guarda (crea o actualiza) un estado de incidencia en la base de datos.
    /// Si el estado ya existe, se actualizan sus propiedades; si no, se agrega como nuevo.
    /// </summary>
    /// <param name="issueState">Entidad <see cref="IssueState"/> a persistir.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de guardado.</returns>
    public async Task SaveAsync(IssueState issueState, CancellationToken cancellationToken = default)
    {
        _context.IssueStates.Update(issueState);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Elimina un estado de incidencia de la base de datos por su identificador.
    /// Si el estado no existe, la operación se completa sin errores.
    /// </summary>
    /// <param name="id">Identificador único del estado de incidencia a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de eliminación.</returns>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.IssueStates
            .FirstOrDefaultAsync(s => s.StateId == id, cancellationToken);

        if (entry is not null)
        {
            _context.IssueStates.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
