using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

/// <summary>
/// Repositorio de Entity Framework Core para la entidad <see cref="KnowledgeTag"/> con operación
/// de búsqueda o creación automática. Implementa la interfaz <see cref="ITagRepository"/>
/// para resolver etiquetas por nombre en la base de datos SQL Server.
/// </summary>
public class EfTagRepository : ITagRepository
{
    private readonly AxiomDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio <see cref="EfTagRepository"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos de Axiom.</param>
    public EfTagRepository(AxiomDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Busca una etiqueta de conocimiento por su nombre exacto. Si no existe, la crea
    /// y la persiste en la base de datos antes de devolverla.
    /// </summary>
    /// <param name="tagName">Nombre de la etiqueta a buscar o crear.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>
    /// La entidad <see cref="KnowledgeTag"/> existente o recién creada.
    /// Nunca devuelve <c>null</c>.
    /// </returns>
    public async Task<KnowledgeTag> FindOrCreateAsync(string tagName, CancellationToken cancellationToken = default)
    {
        var tag = await _context.KnowledgeTags
            .FirstOrDefaultAsync(t => t.TagName == tagName, cancellationToken);

        if (tag is not null)
            return tag;

        tag = new KnowledgeTag(tagName);
        _context.KnowledgeTags.Add(tag);
        await _context.SaveChangesAsync(cancellationToken);

        return tag;
    }
}
