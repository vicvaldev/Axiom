using Axiom.Application.Dtos;

namespace Axiom.Application.Interfaces;

/// <summary>
/// Servicio de búsqueda inteligente sobre la base de conocimiento.
/// Permite realizar consultas en lenguaje natural y obtener resultados relevantes
/// a partir del índice de búsqueda.
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Realiza una consulta en lenguaje natural sobre el índice de conocimiento
    /// y devuelve los resultados más relevantes.
    /// </summary>
    /// <param name="question">Pregunta o cadena de búsqueda en lenguaje natural.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Colección de objetos <see cref="SearchResultDto"/> con los resultados de la búsqueda ordenados por relevancia.</returns>
    Task<IEnumerable<SearchResultDto>> AskAsync(string question, CancellationToken ct = default);
}
