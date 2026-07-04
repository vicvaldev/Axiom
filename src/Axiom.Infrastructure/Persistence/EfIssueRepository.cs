using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

/// <summary>
/// Repositorio de entidades <see cref="Issue"/> implementado sobre Entity Framework Core.
/// Proporciona operaciones CRUD y consultas específicas por sistema (EAI),
/// con carga explícita de relaciones (sistema, estado y creador).
/// </summary>
public class EfIssueRepository : IIssueRepository
{
    private readonly AxiomDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio con el contexto de base de datos especificado.
    /// </summary>
    /// <param name="context">Contexto de Entity Framework Core que expone las tablas del esquema Axiom.</param>
    public EfIssueRepository(AxiomDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Guarda (crea o actualiza) una incidencia en la base de datos.
    /// Si la entidad ya existe (mismo <see cref="Issue.IssueId"/>), actualiza sus valores;
    /// en caso contrario, la agrega como nueva.
    /// </summary>
    /// <param name="issue">Entidad <see cref="Issue"/> a persistir.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>Una tarea que representa la operación asincrónica de guardado.</returns>
    public async Task SaveAsync(Issue issue, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Issues
            .FirstOrDefaultAsync(i => i.IssueId == issue.IssueId, cancellationToken);

        if (existing is not null)
        {
            _context.Entry(existing).CurrentValues.SetValues(issue);
        }
        else
        {
            await _context.Issues.AddAsync(issue, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene una incidencia por su identificador único, incluyendo sus relaciones
    /// (sistema, estado y creador). La consulta se realiza sin seguimiento de cambios.
    /// </summary>
    /// <param name="id">Identificador único de la incidencia (<see cref="Issue.IssueId"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>
    /// La entidad <see cref="Issue"/> si existe; <c>null</c> si no se encuentra ninguna incidencia con el identificador especificado.
    /// </returns>
    public async Task<Issue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Issues
            .AsNoTracking()
            .Include(i => i.System)
            .Include(i => i.State)
            .Include(i => i.CreatedBy)
            .FirstOrDefaultAsync(i => i.IssueId == id, cancellationToken);
    }

    /// <summary>
    /// Obtiene todas las incidencias registradas, incluyendo sus relaciones
    /// (sistema y estado). La consulta se realiza sin seguimiento de cambios.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>Colección de todas las entidades <see cref="Issue"/>.</returns>
    public async Task<IEnumerable<Issue>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Issues
            .AsNoTracking()
            .Include(i => i.System)
            .Include(i => i.State)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Elimina una incidencia de la base de datos por su identificador único.
    /// Si no existe ninguna incidencia con el identificador especificado, la operación se omite.
    /// </summary>
    /// <param name="id">Identificador único de la incidencia a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>Una tarea que representa la operación asincrónica de eliminación.</returns>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Issues
            .FirstOrDefaultAsync(i => i.IssueId == id, cancellationToken);

        if (entry is not null)
        {
            _context.Issues.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Obtiene todas las incidencias asociadas a un sistema identificado por su código EAI.
    /// Incluye las relaciones de sistema y estado. La consulta se realiza sin seguimiento de cambios.
    /// </summary>
    /// <param name="eai">Código EAI del sistema (<see cref="Domain.Entities.AxiomSystem.EAI"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>Colección de incidencias cuyo sistema coincide con el EAI especificado.</returns>
    public async Task<IEnumerable<Issue>> GetByEaiAsync(string eai, CancellationToken cancellationToken = default)
    {
        return await _context.Issues
            .AsNoTracking()
            .Include(i => i.System)
            .Include(i => i.State)
            .Where(i => i.System != null && i.System.EAI == eai)
            .ToListAsync(cancellationToken);
    }
}
