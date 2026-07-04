namespace Axiom.Application.Dtos;

/// <summary>
/// DTO de proyección para un ticket o incidencia. Expone la información más
/// relevante de un issue, incluyendo su estado y sistema asociado, sin
/// navegaciones a otras entidades de dominio.
/// </summary>
public class IssueDto
{
    /// <summary>
    /// Identificador único del issue.
    /// </summary>
    public Guid IssueId { get; init; }

    /// <summary>
    /// Resumen descriptivo del issue.
    /// </summary>
    public string Summary { get; init; } = null!;

    /// <summary>
    /// Nombre del sistema al que pertenece el issue.
    /// </summary>
    public string SystemName { get; init; } = null!;

    /// <summary>
    /// Nombre del estado actual del issue (p. ej., "Abierto", "En curso", "Resuelto").
    /// </summary>
    public string StateName { get; init; } = null!;

    /// <summary>
    /// Número de RITM (Request Item) asociado, si existe. Es único en el sistema.
    /// Puede ser <c>null</c> si no se ha vinculado ningún RITM.
    /// </summary>
    public string? RitmNumber { get; init; }

    /// <summary>
    /// Número de incidencia asociado, si existe. Es único en el sistema.
    /// Puede ser <c>null</c> si no se ha vinculado ninguna incidencia.
    /// </summary>
    public string? IncidentNumber { get; init; }

    /// <summary>
    /// Fecha y hora de creación del issue.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Fecha y hora de resolución del issue. Puede ser <c>null</c> si aún no se ha resuelto.
    /// </summary>
    public DateTime? ResolvedAt { get; init; }
}
