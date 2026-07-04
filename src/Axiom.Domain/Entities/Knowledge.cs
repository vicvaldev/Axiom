namespace Axiom.Domain.Entities;

/// <summary>
/// Representa un artículo de conocimiento técnico dentro del sistema.
/// </summary>
/// <remarks>
/// <para>
/// La entidad <see cref="Knowledge"/> almacena la base de conocimiento del sistema, incluyendo
/// título, resumen y contenido detallado. Cada artículo pertenece a un <see cref="AxiomSystem"/>
/// y es creado por un <see cref="User"/>. Dispone de una clasificación mediante
/// <see cref="KnowledgeType"/> y un estado definido por <see cref="KnowledgeState"/>.
/// </para>
/// <para>
/// Opcionalmente, un artículo puede estar asociado a un <see cref="Issue"/> (incidencia o
/// solicitud) que motivó su creación. La relación muchos-a-muchos con etiquetas
/// (<see cref="KnowledgeTag"/>) se gestiona a través de la colección de unión
/// <see cref="KnowledgeKnowledgeTags"/>.
/// </para>
/// <para>
/// El control de versiones se lleva mediante <see cref="VersionNumber"/>, que se incrementa
/// automáticamente cada vez que se invoca <see cref="Update"/>.
/// </para>
/// </remarks>
public class Knowledge
{
    /// <summary>
    /// Identificador único del artículo de conocimiento.
    /// </summary>
    /// <value>Valor <see cref="Guid"/> generado en el constructor o proporcionado externamente.</value>
    public Guid KnowledgeId { get; private set; }

    /// <summary>
    /// Título del artículo de conocimiento.
    /// </summary>
    /// <value>Cadena no vacía que identifica de forma descriptiva el contenido del artículo.</value>
    public string Title { get; private set; } = null!;

    /// <summary>
    /// Resumen o extracto breve del artículo.
    /// </summary>
    /// <value>Cadena que puede estar vacía si no se proporcionó resumen.</value>
    public string Summary { get; private set; } = null!;

    /// <summary>
    /// Contenido completo del artículo en formato textual.
    /// </summary>
    /// <value>Cadena no vacía con el cuerpo del conocimiento.</value>
    public string Content { get; private set; } = null!;

    /// <summary>
    /// Identificador del <see cref="AxiomSystem"/> al que pertenece este artículo.
    /// </summary>
    /// <value>Clave foránea hacia la entidad <see cref="AxiomSystem"/>.</value>
    public long SystemId { get; private set; }

    /// <summary>
    /// Fecha y hora (UTC) de creación del artículo.
    /// </summary>
    /// <value>Establecida automáticamente en el constructor con <see cref="DateTime.UtcNow"/>.</value>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Fecha y hora (UTC) de la última actualización del artículo.
    /// </summary>
    /// <value>Se actualiza automáticamente en cada llamada a <see cref="Update"/>.</value>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Identificador del <see cref="User"/> que creó el artículo.
    /// </summary>
    /// <value>Clave foránea hacia la entidad <see cref="User"/>.</value>
    public Guid CreatedByUserId { get; private set; }

    /// <summary>
    /// Identificador del <see cref="KnowledgeType"/> que clasifica este artículo.
    /// </summary>
    /// <value>Clave foránea hacia la entidad <see cref="KnowledgeType"/>.</value>
    public long KnowledgeTypeId { get; private set; }

    /// <summary>
    /// Identificador del <see cref="KnowledgeState"/> que indica el estado actual del artículo.
    /// </summary>
    /// <value>Clave foránea hacia la entidad <see cref="KnowledgeState"/>.</value>
    public int KnowledgeStateId { get; private set; }

    /// <summary>
    /// Identificador opcional del <see cref="Issue"/> asociado a este artículo.
    /// </summary>
    /// <value>
    /// Clave foránea nullable hacia la entidad <see cref="Issue"/>. 
    /// <c>null</c> si el artículo no está vinculado a ninguna incidencia o solicitud.
    /// </value>
    public Guid? IssueId { get; private set; }

    /// <summary>
    /// Número de versión del artículo. Se incrementa automáticamente en cada actualización.
    /// </summary>
    /// <value>Comienza en 1 en el constructor y aumenta en 1 por cada llamada a <see cref="Update"/>.</value>
    public int VersionNumber { get; private set; }

    /// <summary>
    /// Navegación al <see cref="AxiomSystem"/> al que pertenece este artículo.
    /// </summary>
    /// <value>Propiedad de navegación asignada por Entity Framework Core.</value>
    public AxiomSystem System { get; private set; } = null!;

    /// <summary>
    /// Navegación al <see cref="User"/> que creó este artículo.
    /// </summary>
    /// <value>Propiedad de navegación asignada por Entity Framework Core.</value>
    public User CreatedBy { get; private set; } = null!;

    /// <summary>
    /// Navegación al <see cref="KnowledgeType"/> que clasifica este artículo.
    /// </summary>
    /// <value>Propiedad de navegación asignada por Entity Framework Core.</value>
    public KnowledgeType Type { get; private set; } = null!;

    /// <summary>
    /// Navegación al <see cref="KnowledgeState"/> que indica el estado actual del artículo.
    /// </summary>
    /// <value>Propiedad de navegación asignada por Entity Framework Core.</value>
    public KnowledgeState State { get; private set; } = null!;

    /// <summary>
    /// Navegación opcional al <see cref="Issue"/> asociado a este artículo.
    /// </summary>
    /// <value><c>null</c> si el artículo no está vinculado a ninguna incidencia o solicitud.</value>
    public Issue? Issue { get; private set; }

    /// <summary>
    /// Colección de asociaciones muchos-a-muchos entre este artículo y sus <see cref="KnowledgeTag"/>.
    /// </summary>
    /// <value>
    /// Conjunto de tipo <see cref="HashSet{T}"/> inicializado por defecto. Cada elemento
    /// <see cref="KnowledgeKnowledgeTag"/> contiene las claves compuestas de la relación.
    /// </value>
    public ICollection<KnowledgeKnowledgeTag> KnowledgeKnowledgeTags { get; private set; } = new HashSet<KnowledgeKnowledgeTag>();

    /// <summary>
    /// Constructor privado sin parámetros requerido por Entity Framework Core para la
    /// materialización de proxies y la deserialización de datos.
    /// </summary>
    /// <remarks>
    /// No debe ser utilizado directamente desde el código de aplicación. Todas las
    /// propiedades permanecen con sus valores predeterminados hasta que EF Core las
    /// asigna durante la hidratación de una consulta.
    /// </remarks>
    private Knowledge() { }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="Knowledge"/> con los valores
    /// proporcionados, estableciendo <see cref="CreatedAt"/> y <see cref="UpdatedAt"/>
    /// a la hora UTC actual, y <see cref="VersionNumber"/> a 1.
    /// </summary>
    /// <param name="title">Título del artículo. No puede ser <c>null</c>, vacío ni contener solo espacios.</param>
    /// <param name="summary">Resumen breve del artículo. Si es <c>null</c>, se almacena como cadena vacía.</param>
    /// <param name="content">Contenido completo del artículo. No puede ser <c>null</c>, vacío ni contener solo espacios.</param>
    /// <param name="systemId">Identificador del <see cref="AxiomSystem"/> al que pertenece el artículo.</param>
    /// <param name="createdByUserId">Identificador del <see cref="User"/> que crea el artículo.</param>
    /// <param name="knowledgeTypeId">Identificador del <see cref="KnowledgeType"/> que clasifica el artículo.</param>
    /// <param name="knowledgeStateId">Identificador del <see cref="KnowledgeState"/> que indica el estado inicial del artículo.</param>
    /// <param name="issueId">
    /// Identificador opcional del <see cref="Issue"/> asociado. <c>null</c> si el artículo
    /// no se vincula a ninguna incidencia o solicitud.
    /// </param>
    /// <param name="knowledgeId">
    /// Identificador único opcional para el artículo. Si no se proporciona, se genera
    /// automáticamente un nuevo <see cref="Guid"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Se lanza cuando <paramref name="title"/> o <paramref name="content"/> son <c>null</c>,
    /// están vacíos o contienen únicamente espacios en blanco.
    /// </exception>
    public Knowledge(
        string title,
        string summary,
        string content,
        long systemId,
        Guid createdByUserId,
        long knowledgeTypeId,
        int knowledgeStateId,
        Guid? issueId = null,
        Guid? knowledgeId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        KnowledgeId = knowledgeId ?? Guid.NewGuid();
        Title = title;
        Summary = summary ?? string.Empty;
        Content = content;
        SystemId = systemId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
        KnowledgeTypeId = knowledgeTypeId;
        KnowledgeStateId = knowledgeStateId;
        IssueId = issueId;
        VersionNumber = 1;
    }

    /// <summary>
    /// Actualiza los datos del artículo de conocimiento, incrementando
    /// <see cref="VersionNumber"/> en 1 y estableciendo <see cref="UpdatedAt"/>
    /// a la hora UTC actual.
    /// </summary>
    /// <param name="title">Nuevo título del artículo. No puede ser <c>null</c>, vacío ni contener solo espacios.</param>
    /// <param name="summary">Nuevo resumen del artículo. Si es <c>null</c>, se almacena como cadena vacía.</param>
    /// <param name="content">Nuevo contenido del artículo. No puede ser <c>null</c>, vacío ni contener solo espacios.</param>
    /// <param name="systemId">Nuevo identificador del <see cref="AxiomSystem"/> asociado.</param>
    /// <param name="knowledgeTypeId">Nuevo identificador del <see cref="KnowledgeType"/> clasificador.</param>
    /// <param name="knowledgeStateId">Nuevo identificador del <see cref="KnowledgeState"/>.</param>
    /// <param name="issueId">
    /// Nuevo identificador opcional del <see cref="Issue"/> asociado.
    /// <c>null</c> para desvincular el artículo de cualquier incidencia o solicitud.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Se lanza cuando <paramref name="title"/> o <paramref name="content"/> son <c>null</c>,
    /// están vacíos o contienen únicamente espacios en blanco.
    /// </exception>
    public void Update(
        string title,
        string summary,
        string content,
        long systemId,
        long knowledgeTypeId,
        int knowledgeStateId,
        Guid? issueId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        Title = title;
        Summary = summary ?? string.Empty;
        Content = content;
        SystemId = systemId;
        KnowledgeTypeId = knowledgeTypeId;
        KnowledgeStateId = knowledgeStateId;
        IssueId = issueId;
        UpdatedAt = DateTime.UtcNow;
        VersionNumber++;
    }
}
