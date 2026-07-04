using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

/// <summary>
/// Repositorio de Entity Framework Core para la entidad <see cref="KnowledgeType"/>.
/// Implementa la interfaz <see cref="IKnowledgeTypeRepository"/> para operaciones de persistencia
/// de tipos de conocimiento en la base de datos SQL Server.
/// </summary>
public class EfKnowledgeTypeRepository : IKnowledgeTypeRepository
{
    private readonly AxiomDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio <see cref="EfKnowledgeTypeRepository"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos de Axiom.</param>
    public EfKnowledgeTypeRepository(AxiomDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene un tipo de conocimiento por su identificador numérico.
    /// </summary>
    /// <param name="id">Identificador único del tipo de conocimiento (<see cref="long"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// La entidad <see cref="KnowledgeType"/> si se encuentra; en caso contrario, <c>null</c>.
    /// </returns>
    public async Task<KnowledgeType?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeTypes
            .FirstOrDefaultAsync(t => t.TypeId == id, cancellationToken);
    }

    /// <summary>
    /// Guarda (crea o actualiza) un tipo de conocimiento en la base de datos.
    /// Si el tipo ya existe, se actualizan sus propiedades; si no, se agrega como nuevo.
    /// </summary>
    /// <param name="knowledgeType">Entidad <see cref="KnowledgeType"/> a persistir.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de guardado.</returns>
    public async Task SaveAsync(KnowledgeType knowledgeType, CancellationToken cancellationToken = default)
    {
        _context.KnowledgeTypes.Update(knowledgeType);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Elimina un tipo de conocimiento de la base de datos por su identificador.
    /// Si el tipo no existe, la operación se completa sin errores.
    /// </summary>
    /// <param name="id">Identificador único del tipo de conocimiento a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de eliminación.</returns>
    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.KnowledgeTypes
            .FirstOrDefaultAsync(t => t.TypeId == id, cancellationToken);

        if (entry is not null)
        {
            _context.KnowledgeTypes.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
