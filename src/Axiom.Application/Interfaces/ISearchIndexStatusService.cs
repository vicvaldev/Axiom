using Axiom.Application.Dtos;

namespace Axiom.Application.Interfaces;

/// <summary>
/// Servicio que proporciona información sobre el estado actual del índice de búsqueda.
/// Permite consultar métricas como la cantidad de documentos indexados, la fecha de
/// la última indexación y el estado de salud del índice.
/// </summary>
public interface ISearchIndexStatusService
{
    /// <summary>
    /// Obtiene el estado actual del índice de búsqueda, incluyendo métricas
    /// y estadísticas de cada tipo de entidad indexada.
    /// </summary>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Colección de objetos <see cref="SearchIndexStatusDto"/> con el estado del índice por entidad.</returns>
    Task<IEnumerable<SearchIndexStatusDto>> GetStatusAsync(CancellationToken ct = default);
}
