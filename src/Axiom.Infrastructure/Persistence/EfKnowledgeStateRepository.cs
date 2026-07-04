using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

/// <summary>
/// Repositorio de Entity Framework Core para la entidad <see cref="KnowledgeState"/>.
/// Implementa la interfaz <see cref="IKnowledgeStateRepository"/> para operaciones de persistencia
/// de estados de conocimiento en la base de datos SQL Server.
/// </summary>
public class EfKnowledgeStateRepository : IKnowledgeStateRepository
{
    private readonly AxiomDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio <see cref="EfKnowledgeStateRepository"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos de Axiom.</param>
    public EfKnowledgeStateRepository(AxiomDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene un estado de conocimiento por su identificador numérico.
    /// </summary>
    /// <param name="id">Identificador único del estado de conocimiento (<see cref="int"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// La entidad <see cref="KnowledgeState"/> si se encuentra; en caso contrario, <c>null</c>.
    /// </returns>
    public async Task<KnowledgeState?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeStates
            .FirstOrDefaultAsync(s => s.StateId == id, cancellationToken);
    }

    /// <summary>
    /// Guarda (crea o actualiza) un estado de conocimiento en la base de datos.
    /// Si el estado ya existe, se actualizan sus propiedades; si no, se agrega como nuevo.
    /// </summary>
    /// <param name="knowledgeState">Entidad <see cref="KnowledgeState"/> a persistir.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de guardado.</returns>
    public async Task SaveAsync(KnowledgeState knowledgeState, CancellationToken cancellationToken = default)
    {
        _context.KnowledgeStates.Update(knowledgeState);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Elimina un estado de conocimiento de la base de datos por su identificador.
    /// Si el estado no existe, la operación se completa sin errores.
    /// </summary>
    /// <param name="id">Identificador único del estado de conocimiento a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de eliminación.</returns>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.KnowledgeStates
            .FirstOrDefaultAsync(s => s.StateId == id, cancellationToken);

        if (entry is not null)
        {
            _context.KnowledgeStates.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
