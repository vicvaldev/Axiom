namespace Axiom.Application.Dtos;

/// <summary>
/// DTO que informa el estado de un índice de búsqueda. Contiene el nombre del
/// índice y la cantidad de documentos indexados actualmente.
/// </summary>
public class SearchIndexStatusDto
{
    /// <summary>
    /// Nombre del índice de búsqueda.
    /// </summary>
    public string IndexName { get; init; } = null!;

    /// <summary>
    /// Número total de documentos actualmente indexados.
    /// </summary>
    public long DocumentCount { get; init; }
}
