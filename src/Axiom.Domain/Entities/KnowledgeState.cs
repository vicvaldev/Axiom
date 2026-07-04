namespace Axiom.Domain.Entities;

/// <summary>
/// Representa el estado en el que puede encontrarse un conocimiento dentro del sistema.
/// </summary>
/// <remarks>
/// Esta entidad forma parte del catálogo de estados y se utiliza para clasificar el ciclo de vida
/// de los conocimientos. Los valores típicos incluyen <c>"Borrador"</c>, <c>"Publicado"</c> y <c>"Archivado"</c>.
/// <para>
/// Mantiene una relación uno a muchos con la entidad <see cref="Knowledge"/>, donde cada estado
/// puede estar asociado a múltiples conocimientos.
/// </para>
/// </remarks>
public class KnowledgeState
{
    /// <summary>
    /// Obtiene el identificador único del estado.
    /// </summary>
    /// <remarks>
    /// Es generado por la base de datos mediante una columna de identidad (<c>int</c>).
    /// </remarks>
    public int StateId { get; private set; }

    /// <summary>
    /// Obtiene el código alfanumérico único que identifica al estado de forma legible.
    /// </summary>
    /// <remarks>
    /// Por ejemplo: <c>"DRAFT"</c>, <c>"PUBLISHED"</c> o <c>"ARCHIVED"</c>.
    /// Este valor no puede ser <c>null</c> ni estar vacío.
    /// </remarks>
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Obtiene el nombre descriptivo del estado.
    /// </summary>
    /// <remarks>
    /// Por ejemplo: <c>"Borrador"</c>, <c>"Publicado"</c> o <c>"Archivado"</c>.
    /// Este valor no puede ser <c>null</c> ni estar vacío.
    /// </remarks>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Obtiene la colección de conocimientos que se encuentran en este estado.
    /// </summary>
    /// <remarks>
    /// Propiedad de navegación para la relación uno a muchos con <see cref="Knowledge"/>.
    /// Se inicializa como un <see cref="HashSet{T}"/> vacío para evitar referencias <c>null</c>.
    /// Solo se debe acceder a través de <c>.Include()</c> en las consultas de lectura; no se admite
    /// la carga diferida (<i>lazy loading</i>).
    /// </remarks>
    public ICollection<Knowledge> Knowledges { get; private set; } = new HashSet<Knowledge>();

    /// <summary>
    /// Constructor privado sin parámetros requerido por Entity Framework Core para la
    /// deserialización de proxies y materialización de consultas.
    /// </summary>
    /// <remarks>
    /// No debe ser utilizado directamente desde el código de aplicación. Los miembros de solo
    /// lectura o con <c>private set</c> se inicializan a través de este constructor mediante
    /// reflexión por parte de EF Core.
    /// </remarks>
    private KnowledgeState() { }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="KnowledgeState"/> con un código
    /// y un nombre descriptivo.
    /// </summary>
    /// <param name="code">Código único que identifica el estado (por ejemplo, <c>"DRAFT"</c>).
    /// No puede ser <c>null</c>, estar vacío ni contener solo espacios en blanco.</param>
    /// <param name="name">Nombre descriptivo del estado (por ejemplo, <c>"Borrador"</c>).
    /// No puede ser <c>null</c>, estar vacío ni contener solo espacios en blanco.</param>
    /// <exception cref="ArgumentException">
    /// Se produce cuando <paramref name="code"/> o <paramref name="name"/> son <c>null</c>,
    /// están vacíos o contienen únicamente espacios en blanco.
    /// </exception>
    public KnowledgeState(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Code = code;
        Name = name;
    }

    /// <summary>
    /// Actualiza el código y el nombre descriptivo del estado.
    /// </summary>
    /// <remarks>
    /// Este método permite modificar los valores de <see cref="Code"/> y <see cref="Name"/>
    /// una vez que la entidad ha sido creada. Ambos parámetros son validados para garantizar
    /// que no estén vacíos.
    /// </remarks>
    /// <param name="code">Nuevo código único para el estado. No puede ser <c>null</c>, estar vacío
    /// ni contener solo espacios en blanco.</param>
    /// <param name="name">Nuevo nombre descriptivo para el estado. No puede ser <c>null</c>, estar vacío
    /// ni contener solo espacios en blanco.</param>
    /// <exception cref="ArgumentException">
    /// Se produce cuando <paramref name="code"/> o <paramref name="name"/> son <c>null</c>,
    /// están vacíos o contienen únicamente espacios en blanco.
    /// </exception>
    public void Update(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Code = code;
        Name = name;
    }
}
