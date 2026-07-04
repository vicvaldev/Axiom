namespace Axiom.Domain.Entities;

/// <summary>
/// Entidad de unión (join) para la relación muchos a muchos entre <see cref="Knowledge"/> y <see cref="KnowledgeTag"/>.
/// Cada instancia representa la asignación de una etiqueta a un conocimiento concreto.
/// </summary>
/// <remarks>
/// La tabla intermedia <c>KnowledgeKnowledgeTags</c> se compone de las claves foráneas
/// <see cref="KnowledgeId"/> y <see cref="KnowledgeTagId"/>, formando una clave primaria compuesta.
/// El borrado en cascada está habilitado en ambas relaciones.
/// Esta clase no expone setters públicos para evitar modificaciones inconsistentes de la relación.
/// </remarks>
public class KnowledgeKnowledgeTag
{
    /// <summary>
    /// Identificador del conocimiento al que se asigna la etiqueta.
    /// </summary>
    /// <remarks>
    /// Forma parte de la clave primaria compuesta junto con <see cref="KnowledgeTagId"/>.
    /// Es un <see cref="Guid"/> generado en el dominio y asignado por la entidad <see cref="Knowledge"/>.
    /// </remarks>
    public Guid KnowledgeId { get; private set; }

    /// <summary>
    /// Identificador de la etiqueta asignada al conocimiento.
    /// </summary>
    /// <remarks>
    /// Forma parte de la clave primaria compuesta junto con <see cref="KnowledgeId"/>.
    /// Es un valor <see cref="long"/> generado por la base de datos (identidad) en la entidad <see cref="KnowledgeTag"/>.
    /// </remarks>
    public long KnowledgeTagId { get; private set; }

    /// <summary>
    /// Propiedad de navegación hacia la entidad <see cref="Knowledge"/> asociada.
    /// </summary>
    /// <remarks>
    /// Esta propiedad puede ser <c>null</c> si la relación no se ha cargado explícitamente mediante <c>Include</c> o <c>ThenInclude</c>.
    /// No se utiliza carga diferida (lazy loading); el valor debe obtenerse mediante una consulta explícita con <c>AsNoTracking</c> para operaciones de solo lectura.
    /// </remarks>
    public Knowledge? Knowledge { get; private set; }

    /// <summary>
    /// Propiedad de navegación hacia la entidad <see cref="KnowledgeTag"/> asociada.
    /// </summary>
    /// <remarks>
    /// Esta propiedad puede ser <c>null</c> si la relación no se ha cargado explícitamente mediante <c>Include</c> o <c>ThenInclude</c>.
    /// No se utiliza carga diferida (lazy loading); el valor debe obtenerse mediante una consulta explícita con <c>AsNoTracking</c> para operaciones de solo lectura.
    /// </remarks>
    public KnowledgeTag? Tag { get; private set; }

    /// <summary>
    /// Constructor privado sin parámetros requerido por Entity Framework Core para la materialización de entidades desde la base de datos.
    /// </summary>
    /// <remarks>
    /// Este constructor no debe ser utilizado directamente desde el código de aplicación.
    /// Entity Framework Core lo usa internamente cuando hidrata instancias a partir de los resultados de una consulta.
    /// </remarks>
    private KnowledgeKnowledgeTag() { }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="KnowledgeKnowledgeTag"/> con los identificadores del conocimiento y la etiqueta.
    /// </summary>
    /// <param name="knowledgeId">El identificador único (<see cref="Guid"/>) del conocimiento al que se asigna la etiqueta.</param>
    /// <param name="knowledgeTagId">El identificador único (<see cref="long"/>) de la etiqueta que se asigna al conocimiento.</param>
    /// <remarks>
    /// Este constructor es el punto de entrada del dominio para crear una relación entre un conocimiento y una etiqueta.
    /// Los valores proporcionados deben corresponder a entidades existentes en la base de datos; de lo contrario,
    /// se producirá una infracción de clave foránea al guardar los cambios.
    /// </remarks>
    public KnowledgeKnowledgeTag(Guid knowledgeId, long knowledgeTagId)
    {
        KnowledgeId = knowledgeId;
        KnowledgeTagId = knowledgeTagId;
    }
}
