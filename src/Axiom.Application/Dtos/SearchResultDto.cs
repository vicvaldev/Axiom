namespace Axiom.Application.Dtos;

/// <summary>
/// DTO que representa un resultado de búsqueda unificado. Puede corresponder
/// a distintas entidades (conocimientos, incidencias, etc.) y contiene un
/// puntaje de relevancia junto con un fragmento resaltado opcional.
/// </summary>
public class SearchResultDto
{
    /// <summary>
    /// Identificador único del resultado (p. ej., un GUID de conocimiento o issue).
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Tipo de entidad a la que pertenece el resultado (p. ej., "Knowledge", "Issue").
    /// </summary>
    public string EntityType { get; init; } = null!;

    /// <summary>
    /// Título del resultado de búsqueda.
    /// </summary>
    public string Title { get; init; } = null!;

    /// <summary>
    /// Resumen o extracto del contenido del resultado.
    /// </summary>
    public string Summary { get; init; } = null!;

    /// <summary>
    /// Nombre del sistema asociado al resultado.
    /// </summary>
    public string SystemName { get; init; } = null!;

    /// <summary>
    /// Nombre del estado del resultado.
    /// </summary>
    public string StateName { get; init; } = null!;

    /// <summary>
    /// Puntaje de relevancia del resultado en la búsqueda. Los valores más altos
    /// indican una mayor coincidencia con la consulta.
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// Fecha y hora de la última actualización del resultado.
    /// </summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Fragmento de texto resaltado que muestra el contexto donde aparece el
    /// término buscado. Puede ser <c>null</c> si no se generó un resaltado.
    /// </summary>
    public string? Highlight { get; init; }
}
