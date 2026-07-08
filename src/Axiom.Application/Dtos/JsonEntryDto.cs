namespace Axiom.Application.Dtos;

/// <summary>
/// DTO para la serialización/deserialización JSON de un conocimiento.
/// Utilizado en operaciones de semilla (seed), importación/exportación y
/// respuestas de API. Contiene todas las propiedades del conocimiento
/// con identificadores de las entidades relacionadas.
/// </summary>
public class JsonKnowledgeEntry
{
    /// <summary>
    /// Identificador único del conocimiento.
    /// </summary>
    public Guid KnowledgeId { get; set; }

    /// <summary>
    /// Título del artículo de conocimiento.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Resumen o extracto breve del contenido del conocimiento.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Contenido completo del artículo de conocimiento.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del sistema al que pertenece el conocimiento.
    /// </summary>
    public long SystemId { get; set; }

    /// <summary>
    /// Identificador del usuario que creó el conocimiento.
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    /// <summary>
    /// Identificador del tipo de conocimiento.
    /// </summary>
    public long KnowledgeTypeId { get; set; }

    /// <summary>
    /// Identificador del estado del conocimiento.
    /// </summary>
    public int KnowledgeStateId { get; set; }

    /// <summary>
    /// Identificador del issue asociado al conocimiento, si existe.
    /// Puede ser <c>null</c> si el conocimiento no está vinculado a ningún issue.
    /// </summary>
    public Guid? IssueId { get; set; }

    /// <summary>
    /// Lista de etiquetas asociadas al conocimiento.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Número de versión actual del conocimiento.
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Fecha y hora de creación del conocimiento.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha y hora de la última actualización del conocimiento.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO para la serialización/deserialización JSON de un issue.
/// Utilizado en operaciones de semilla (seed), importación/exportación y
/// respuestas de API. Contiene identificadores de las entidades relacionadas.
/// </summary>
public class JsonIssueEntry
{
    /// <summary>
    /// Identificador único del issue.
    /// </summary>
    public Guid IssueId { get; set; }

    /// <summary>
    /// Resumen descriptivo del issue.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del sistema al que pertenece el issue.
    /// </summary>
    public long SystemId { get; set; }

    /// <summary>
    /// Descripción detallada del problema.
    /// </summary>
    public string Problem { get; set; } = string.Empty;

    /// <summary>
    /// Análisis o diagnóstico realizado sobre el issue.
    /// </summary>
    public string Analysis { get; set; } = string.Empty;

    /// <summary>
    /// Resolución aplicada al issue.
    /// </summary>
    public string Resolution { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del estado actual del issue.
    /// </summary>
    public int StateId { get; set; }

    /// <summary>
    /// Identificador del usuario que creó el issue.
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    /// <summary>
    /// Número de RITM (Request Item) asociado, si existe. Puede ser <c>null</c>.
    /// </summary>
    public string? RitmNumber { get; set; }

    /// <summary>
    /// Número de incidencia asociado, si existe. Puede ser <c>null</c>.
    /// </summary>
    public string? IncidentNumber { get; set; }

    /// <summary>
    /// Fecha y hora de creación del issue.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha y hora de la última actualización del issue.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Fecha y hora de resolución del issue. Puede ser <c>null</c> si aún no se ha resuelto.
    /// </summary>
    public DateTime? ResolvedAt { get; set; }
}

/// <summary>
/// DTO para la serialización/deserialización JSON de un usuario.
/// Utilizado en operaciones de semilla (seed) e importación/exportación.
/// </summary>
public class JsonUserEntry
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Dirección de correo electrónico del usuario.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// DTO para la serialización/deserialización JSON de un sistema.
/// Utilizado en operaciones de semilla (seed) e importación/exportación.
/// </summary>
public class JsonSystemEntry
{
    /// <summary>
    /// Identificador único del sistema.
    /// </summary>
    public long SystemId { get; set; }

    /// <summary>
    /// Código EAI del sistema.
    /// </summary>
    public string EAI { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo del sistema.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del usuario propietario del sistema.
    /// </summary>
    public Guid OwnerUserId { get; set; }
}

/// <summary>
/// DTO para la serialización/deserialización JSON de un tipo de conocimiento.
/// Utilizado en operaciones de semilla (seed) e importación/exportación.
/// </summary>
public class JsonKnowledgeTypeEntry
{
    /// <summary>
    /// Identificador numérico del tipo de conocimiento.
    /// </summary>
    public long TypeId { get; set; }

    /// <summary>
    /// Código único del tipo de conocimiento (p. ej., "GUIDE", "FAQ").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo del tipo de conocimiento.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// DTO para la serialización/deserialización JSON de un estado de conocimiento.
/// Utilizado en operaciones de semilla (seed) e importación/exportación.
/// </summary>
public class JsonKnowledgeStateEntry
{
    /// <summary>
    /// Identificador numérico del estado de conocimiento.
    /// </summary>
    public int StateId { get; set; }

    /// <summary>
    /// Código único del estado de conocimiento (p. ej., "DRAFT", "PUBLISHED").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo del estado de conocimiento.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// DTO para la serialización/deserialización JSON de un estado de issue.
/// Utilizado en operaciones de semilla (seed) e importación/exportación.
/// </summary>
public class JsonIssueStateEntry
{
    /// <summary>
    /// Identificador numérico del estado de issue.
    /// </summary>
    public int StateId { get; set; }

    /// <summary>
    /// Código único del estado de issue (p. ej., "OPEN", "IN_PROGRESS", "RESOLVED").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo del estado de issue.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// DTO para la serialización/deserialización JSON de una etiqueta de conocimiento.
/// Utilizado en operaciones de semilla (seed) e importación/exportación.
/// </summary>
public class JsonKnowledgeTagEntry
{
    /// <summary>
    /// Identificador numérico de la etiqueta.
    /// </summary>
    public long KnowledgeTagId { get; set; }

    /// <summary>
    /// Nombre único de la etiqueta (p. ej., "rendimiento", "seguridad").
    /// </summary>
    public string TagName { get; set; } = string.Empty;
}

/// <summary>
/// DTO para la serialización/deserialización JSON de un componente técnico.
/// </summary>
public class JsonTechnicalComponentEntry
{
    public Guid ComponentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TechnicalName { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Criticality { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long SystemId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO para la serialización/deserialización JSON de la relación sistema-componente.
/// </summary>
public class JsonSystemComponentEntry
{
    public Guid SystemComponentId { get; set; }
    public long SystemId { get; set; }
    public Guid ComponentId { get; set; }
}

/// <summary>
/// DTO para la serialización/deserialización JSON de una dependencia entre componentes.
/// </summary>
public class JsonComponentDependencyEntry
{
    public Guid DependencyId { get; set; }
    public Guid SourceComponentId { get; set; }
    public Guid TargetComponentId { get; set; }
    public string DependencyType { get; set; } = string.Empty;
    public string Criticality { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// DTO para la serialización/deserialización JSON de un evento de trazabilidad.
/// </summary>
public class JsonDependencyTraceEventEntry
{
    public Guid TraceEventId { get; set; }
    public Guid DependencyId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? IssueId { get; set; }
    public Guid? KnowledgeId { get; set; }
    public string? RitmNumber { get; set; }
    public string? ChangeNumber { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
