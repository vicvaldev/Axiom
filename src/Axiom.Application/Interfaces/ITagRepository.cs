using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

/// <summary>
/// Repositorio para la gestión de etiquetas de conocimiento (<see cref="KnowledgeTag"/>).
/// Proporciona operaciones para buscar etiquetas existentes o crearlas cuando no existen,
/// siguiendo el patrón "find-or-create".
/// </summary>
public interface ITagRepository
{
    /// <summary>
    /// Busca una etiqueta por su nombre. Si no existe, la crea y la persiste.
    /// </summary>
    /// <param name="tagName">Nombre de la etiqueta a buscar o crear. No debe ser nulo ni estar vacío.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>La etiqueta <see cref="KnowledgeTag"/> existente o recién creada.</returns>
    Task<KnowledgeTag> FindOrCreateAsync(string tagName, CancellationToken cancellationToken = default);
}
