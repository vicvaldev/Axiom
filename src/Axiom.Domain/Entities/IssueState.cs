namespace Axiom.Domain.Entities;

/// <summary>
/// Representa el estado de una incidencia dentro del sistema.
/// </summary>
/// <remarks>
/// Esta entidad actúa como un catálogo de valores para los estados que puede
/// tomar una incidencia (por ejemplo: "Abierto", "En Progreso", "Resuelto",
/// "Cerrado", "Rechazado").
/// <para/>
/// Mantiene una relación uno a muchos con <see cref="Issue"/>: cada estado
/// puede estar asociado a varias incidencias, mientras que cada incidencia
/// pertenece a un único estado.
/// </remarks>
public class IssueState
{
    /// <summary>
    /// Obtiene el identificador único del estado.
    /// </summary>
    /// <remarks>
    /// Corresponde a la clave primaria en la tabla de base de datos.
    /// Es generado automáticamente mediante identity.
    /// El <c>set</c> privado garantiza la inmutabilidad del identificador
    /// una vez creada la entidad, permitiendo únicamente su asignación
    /// por parte del constructor de Entity Framework Core.
    /// </remarks>
    public int StateId { get; private set; }

    /// <summary>
    /// Obtiene el código único y descriptivo que identifica al estado.
    /// </summary>
    /// <remarks>
    /// Se utiliza como identificador semántico dentro del dominio
    /// (por ejemplo: <c>"OPEN"</c>, <c>"IN_PROGRESS"</c>, <c>"RESOLVED"</c>).
    /// Este valor debe ser único en el sistema y no puede ser <see langword="null"/>
    /// ni estar vacío.
    /// </remarks>
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Obtiene el nombre descriptivo y legible del estado.
    /// </summary>
    /// <remarks>
    /// Corresponde al texto visible que se muestra en la interfaz de usuario
    /// (por ejemplo: "Abierto", "En Progreso", "Resuelto").
    /// No puede ser <see langword="null"/> ni estar vacío.
    /// </remarks>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Obtiene la colección de incidencias asociadas a este estado.
    /// </summary>
    /// <remarks>
    /// Representa el extremo "uno" de la relación uno a muchos entre
    /// <see cref="IssueState"/> e <see cref="Issue"/>.
    /// Se inicializa como un <see cref="HashSet{T}"/> vacío para evitar
    /// referencias nulas y permitir la adición diferida de elementos.
    /// El <c>set</c> privado impide la sustitución accidental de la colección
    /// desde fuera de la entidad.
    /// </remarks>
    public ICollection<Issue> Issues { get; private set; } = new HashSet<Issue>();

    /// <summary>
    /// Constructor privado sin parámetros requerido por Entity Framework Core.
    /// </summary>
    /// <remarks>
    /// <see cref="IssueState()"/> es utilizado exclusivamente por EF Core
    /// durante la reconstrucción de entidades desde la base de datos
    /// (materialización de consultas). No debe ser invocado directamente
    /// desde el código de dominio o aplicación.
    /// </remarks>
    private IssueState() { }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IssueState"/> con los
    /// valores de código y nombre especificados.
    /// </summary>
    /// <param name="code">Código único que identifica al estado.
    /// No puede ser <see langword="null"/>, estar vacío ni contener
    /// únicamente espacios en blanco.</param>
    /// <param name="name">Nombre descriptivo del estado.
    /// No puede ser <see langword="null"/>, estar vacío ni contener
    /// únicamente espacios en blanco.</param>
    /// <exception cref="ArgumentException">
    /// Se produce cuando <paramref name="code"/> o <paramref name="name"/>
    /// son <see langword="null"/>, están vacíos o contienen únicamente
    /// espacios en blanco.
    /// </exception>
    public IssueState(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Code = code;
        Name = name;
    }

    /// <summary>
    /// Actualiza el código y el nombre del estado.
    /// </summary>
    /// <remarks>
    /// Este método permite modificar los valores de una instancia existente
    /// de <see cref="IssueState"/>. Realiza las mismas validaciones que el
    /// constructor para garantizar que el estado nunca quede en un estado
    /// inconsistente.
    /// </remarks>
    /// <param name="code">Nuevo código único del estado.
    /// No puede ser <see langword="null"/>, estar vacío ni contener
    /// únicamente espacios en blanco.</param>
    /// <param name="name">Nuevo nombre descriptivo del estado.
    /// No puede ser <see langword="null"/>, estar vacío ni contener
    /// únicamente espacios en blanco.</param>
    /// <exception cref="ArgumentException">
    /// Se produce cuando <paramref name="code"/> o <paramref name="name"/>
    /// son <see langword="null"/>, están vacíos o contienen únicamente
    /// espacios en blanco.
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
