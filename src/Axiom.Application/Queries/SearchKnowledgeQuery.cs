using Axiom.Application.Dtos;
using MediatR;

namespace Axiom.Application.Queries;

/// <summary>
///     Consulta para buscar conocimientos cuyo contenido o título coincida con el texto
///     especificado. Realiza una búsqueda textual sobre el repositorio de conocimientos.
/// </summary>
/// <param name="Query">
///     Término de búsqueda utilizado para filtrar los conocimientos. Se compara contra el título,
///     resumen y contenido de cada conocimiento.
/// </param>
/// <returns>
///     Colección de objetos <see cref="KnowledgeDto"/> que coinciden con el criterio de búsqueda.
/// </returns>
public record SearchKnowledgeQuery(string Query) : IRequest<IEnumerable<KnowledgeDto>>;
