using Axiom.Application.Dtos;
using MediatR;

namespace Axiom.Application.Queries;

/// <summary>
///     Consulta para realizar una pregunta en lenguaje natural sobre la base de conocimiento.
///     El sistema interpreta la pregunta y devuelve los resultados de búsqueda más relevantes
///     encontrados en los conocimientos registrados.
/// </summary>
/// <param name="Question">
///     Texto de la pregunta formulada en lenguaje natural. El sistema analiza su contenido
///     para determinar los términos de búsqueda más relevantes.
/// </param>
/// <returns>
///     Colección de objetos <see cref="SearchResultDto"/> con los resultados más relevantes
///     encontrados para la pregunta formulada.
/// </returns>
public record AskQuery(string Question) : IRequest<IEnumerable<SearchResultDto>>;
