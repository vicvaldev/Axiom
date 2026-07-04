namespace Axiom.Application.Dtos;

/// <summary>
/// DTO de proyección para un artículo de conocimiento. Contiene la información
/// resumida de un conocimiento, incluyendo nombres descriptivos en lugar de
/// identificadores o navegaciones a entidades relacionadas.
/// </summary>
public class KnowledgeDto
{
    /// <summary>
    /// Identificador único del conocimiento.
    /// </summary>
    public Guid KnowledgeId { get; init; }

    /// <summary>
    /// Título del artículo de conocimiento.
    /// </summary>
    public string Title { get; init; } = null!;

    /// <summary>
    /// Resumen o extracto breve del contenido del conocimiento.
    /// </summary>
    public string Summary { get; init; } = null!;

    /// <summary>
    /// Nombre del sistema al que pertenece el conocimiento.
    /// </summary>
    public string SystemName { get; init; } = null!;

    /// <summary>
    /// Lista de etiquetas asociadas al conocimiento.
    /// </summary>
    public List<string> Tags { get; init; } = [];

    /// <summary>
    /// Nombre del tipo de conocimiento (p. ej., "Guía", "Procedimiento", "FAQ").
    /// </summary>
    public string TypeName { get; init; } = null!;

    /// <summary>
    /// Nombre del estado del conocimiento (p. ej., "Borrador", "Publicado", "Archivado").
    /// </summary>
    public string StateName { get; init; } = null!;

    /// <summary>
    /// Nombre del usuario que creó el conocimiento.
    /// </summary>
    public string CreatedByName { get; init; } = null!;

    /// <summary>
    /// Número de versión actual del conocimiento.
    /// </summary>
    public int VersionNumber { get; init; }

    /// <summary>
    /// Fecha y hora de la última actualización del conocimiento.
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
