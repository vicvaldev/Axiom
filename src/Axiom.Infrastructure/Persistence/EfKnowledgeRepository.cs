using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

/// <summary>
/// Repositorio de entidades <see cref="Knowledge"/> implementado sobre Entity Framework Core.
/// Proporciona operaciones CRUD completas con carga explícita de relaciones
/// (sistema, creador, tipo, estado y etiquetas) y búsqueda por texto.
/// </summary>
public class EfKnowledgeRepository : IKnowledgeRepository
{
    private readonly AxiomDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio con el contexto de base de datos especificado.
    /// </summary>
    /// <param name="context">Contexto de Entity Framework Core que expone las tablas del esquema Axiom.</param>
    public EfKnowledgeRepository(AxiomDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Guarda (crea o actualiza) una entrada de conocimiento en la base de datos.
    /// Si la entidad ya existe (mismo <see cref="Knowledge.KnowledgeId"/>), actualiza sus propiedades
    /// y reemplaza la colección de etiquetas asociadas; en caso contrario, la agrega como nueva.
    /// </summary>
    /// <param name="entry">Entidad <see cref="Knowledge"/> a persistir.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>Una tarea que representa la operación asincrónica de guardado.</returns>
    public async Task SaveAsync(Knowledge entry, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Knowledges
            .Include(e => e.KnowledgeKnowledgeTags)
            .FirstOrDefaultAsync(e => e.KnowledgeId == entry.KnowledgeId, cancellationToken);

        if (existing is not null)
        {
            _context.Entry(existing).CurrentValues.SetValues(entry);

            existing.KnowledgeKnowledgeTags.Clear();
            foreach (var tag in entry.KnowledgeKnowledgeTags)
            {
                existing.KnowledgeKnowledgeTags.Add(tag);
            }
        }
        else
        {
            await _context.Knowledges.AddAsync(entry, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene un conocimiento por su identificador único, incluyendo todas sus relaciones
    /// (sistema, creador, tipo, estado y etiquetas). La consulta se realiza sin seguimiento de cambios.
    /// </summary>
    /// <param name="id">Identificador único del conocimiento (<see cref="Knowledge.KnowledgeId"/>).</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>
    /// La entidad <see cref="Knowledge"/> si existe; <c>null</c> si no se encuentra ninguna entrada con el identificador especificado.
    /// </returns>
    public async Task<Knowledge?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Knowledges
            .AsNoTracking()
            .Include(k => k.System)
            .Include(k => k.CreatedBy)
            .Include(k => k.Type)
            .Include(k => k.State)
            .Include(k => k.KnowledgeKnowledgeTags)
                .ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(k => k.KnowledgeId == id, cancellationToken);
    }

    /// <summary>
    /// Obtiene todos los conocimientos registrados, incluyendo sus relaciones
    /// (sistema, creador, tipo, estado y etiquetas). La consulta se realiza sin seguimiento de cambios.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>Colección de todas las entidades <see cref="Knowledge"/>.</returns>
    public async Task<IEnumerable<Knowledge>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Knowledges
            .AsNoTracking()
            .Include(k => k.System)
            .Include(k => k.CreatedBy)
            .Include(k => k.Type)
            .Include(k => k.State)
            .Include(k => k.KnowledgeKnowledgeTags)
                .ThenInclude(t => t.Tag)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca conocimientos cuyo título, resumen o contenido contengan el texto de la consulta.
    /// Incluye todas las relaciones (sistema, creador, tipo, estado y etiquetas).
    /// Si la consulta está vacía o solo contiene espacios, retorna una colección vacía.
    /// </summary>
    /// <param name="query">Texto de búsqueda a comparar contra <c>Title</c>, <c>Summary</c> y <c>Content</c>.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>
    /// Colección de entidades <see cref="Knowledge"/> que coinciden con el criterio de búsqueda.
    /// </returns>
    public async Task<IEnumerable<Knowledge>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<Knowledge>();

        return await _context.Knowledges
            .AsNoTracking()
            .Include(k => k.System)
            .Include(k => k.CreatedBy)
            .Include(k => k.Type)
            .Include(k => k.State)
            .Include(k => k.KnowledgeKnowledgeTags)
                .ThenInclude(t => t.Tag)
            .Where(k =>
                k.Title.Contains(query) ||
                k.Summary.Contains(query) ||
                k.Content.Contains(query))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Elimina un conocimiento de la base de datos por su identificador único.
    /// Si no existe ninguna entrada con el identificador especificado, la operación se omite.
    /// </summary>
    /// <param name="id">Identificador único del conocimiento a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>Una tarea que representa la operación asincrónica de eliminación.</returns>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Knowledges
            .FirstOrDefaultAsync(e => e.KnowledgeId == id, cancellationToken);

        if (entry is not null)
        {
            _context.Knowledges.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
