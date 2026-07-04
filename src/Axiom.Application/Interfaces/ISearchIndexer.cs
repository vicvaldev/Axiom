namespace Axiom.Application.Interfaces;

/// <summary>
/// Indexador del motor de búsqueda. Responsable de mantener sincronizado el índice
/// de búsqueda con los cambios realizados en las entidades de conocimiento e incidencias.
/// Proporciona operaciones para indexar, eliminar del índice y reindexar por completo.
/// </summary>
public interface ISearchIndexer
{
    /// <summary>
    /// Indexa un conocimiento en el motor de búsqueda a partir de su identificador.
    /// </summary>
    /// <param name="id">Identificador único del conocimiento (<see cref="Guid"/>) a indexar.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    Task IndexKnowledgeAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Indexa una incidencia en el motor de búsqueda a partir de su identificador.
    /// </summary>
    /// <param name="id">Identificador único de la incidencia (<see cref="Guid"/>) a indexar.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    Task IndexIssueAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Elimina un conocimiento del índice de búsqueda.
    /// </summary>
    /// <param name="id">Identificador único del conocimiento (<see cref="Guid"/>) a eliminar del índice.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    Task DeleteKnowledgeAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Elimina una incidencia del índice de búsqueda.
    /// </summary>
    /// <param name="id">Identificador único de la incidencia (<see cref="Guid"/>) a eliminar del índice.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    Task DeleteIssueAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Reconstruye el índice de búsqueda completo desde cero, reprocesando
    /// todas las entidades de conocimiento e incidencias existentes.
    /// </summary>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    Task ReindexAllAsync(CancellationToken ct = default);
}
