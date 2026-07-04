using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

/// <summary>
/// Repositorio de Entity Framework Core para la entidad <see cref="KnowledgeTag"/>.
/// Implementa la interfaz <see cref="IKnowledgeTagRepository"/> para operaciones de persistencia
/// de etiquetas de conocimiento en la base de datos SQL Server.
/// </summary>
public class EfKnowledgeTagRepository : IKnowledgeTagRepository
{
    private readonly AxiomDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio <see cref="EfKnowledgeTagRepository"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos de Axiom.</param>
    public EfKnowledgeTagRepository(AxiomDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene una etiqueta de conocimiento por su identificador numérico.
    /// </summary>
    /// <param name="id">Identificador único de la etiqueta (<see cref="long"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// La entidad <see cref="KnowledgeTag"/> si se encuentra; en caso contrario, <c>null</c>.
    /// </returns>
    public async Task<KnowledgeTag?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeTags
            .FirstOrDefaultAsync(t => t.KnowledgeTagId == id, cancellationToken);
    }

    /// <summary>
    /// Guarda (crea o actualiza) una etiqueta de conocimiento en la base de datos.
    /// Si la etiqueta ya existe, se actualizan sus propiedades; si no, se agrega como nueva.
    /// </summary>
    /// <param name="tag">Entidad <see cref="KnowledgeTag"/> a persistir.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de guardado.</returns>
    public async Task SaveAsync(KnowledgeTag tag, CancellationToken cancellationToken = default)
    {
        _context.KnowledgeTags.Update(tag);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene todas las etiquetas de conocimiento ordenadas alfabéticamente por su nombre.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// Una lista de todas las entidades <see cref="KnowledgeTag"/> ordenadas por <see cref="KnowledgeTag.TagName"/>.
    /// </returns>
    public async Task<List<KnowledgeTag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeTags
            .OrderBy(t => t.TagName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Elimina una etiqueta de conocimiento de la base de datos por su identificador.
    /// Si la etiqueta no existe, la operación se completa sin errores.
    /// </summary>
    /// <param name="id">Identificador único de la etiqueta a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de eliminación.</returns>
    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.KnowledgeTags
            .FirstOrDefaultAsync(t => t.KnowledgeTagId == id, cancellationToken);

        if (entry is not null)
        {
            _context.KnowledgeTags.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
