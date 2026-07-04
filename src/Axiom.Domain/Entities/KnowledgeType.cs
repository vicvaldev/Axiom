namespace Axiom.Domain.Entities;

/// <summary>
/// Representa un tipo o categoría de conocimiento dentro del sistema.
/// </summary>
/// <remarks>
/// Los tipos de conocimiento permiten clasificar los distintos artefactos de conocimiento
/// (por ejemplo: "Guía", "FAQ", "Tutorial", "Procedimiento", "Referencia Técnica").
/// Cada <see cref="Knowledge"/> debe estar asociado a un único <see cref="KnowledgeType"/>,
/// estableciendo una relación uno a muchos.
/// </remarks>
public class KnowledgeType
{
    /// <summary>
    /// Identificador único numérico del tipo de conocimiento (clave primaria, autoincremental).
    /// </summary>
    public long TypeId { get; private set; }

    /// <summary>
    /// Código alfanumérico único e identificador del tipo de conocimiento.
    /// </summary>
    /// <remarks>
    /// Se utiliza como referencia estable en integraciones y búsquedas por clave,
    /// independientemente del nombre visible.
    /// </remarks>
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Nombre descriptivo del tipo de conocimiento en lenguaje natural.
    /// </summary>
    /// <remarks>
    /// Es el valor que se muestra en la interfaz de usuario para identificar la categoría
    /// (ejemplo: "Guía", "FAQ", "Tutorial").
    /// </remarks>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Colección de conocimientos (<see cref="Knowledge"/>) que pertenecen a este tipo.
    /// </summary>
    /// <remarks>
    /// Propiedad de navegación que refleja la relación uno a muchos desde <see cref="KnowledgeType"/>
    /// hacia <see cref="Knowledge"/>. Se inicializa como un <see cref="HashSet{T}"/> para evitar
    /// duplicados y mejorar el rendimiento en operaciones de conjunto.
    /// Esta propiedad tiene setter privado para mantener la integridad del dominio.
    /// </remarks>
    public ICollection<Knowledge> Knowledges { get; private set; } = new HashSet<Knowledge>();

    /// <summary>
    /// Constructor privado sin parámetros requerido por Entity Framework Core para la
    /// deserialización de entidades desde la base de datos.
    /// </summary>
    /// <remarks>
    /// No debe ser utilizado directamente desde el código de la aplicación.
    /// Para crear nuevas instancias utilice el constructor público
    /// <see cref="KnowledgeType(string, string)"/>.
    /// </remarks>
    private KnowledgeType() { }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="KnowledgeType"/> con un código
    /// y un nombre.
    /// </summary>
    /// <param name="code">Código alfanumérico único del tipo de conocimiento. No puede ser
    /// <c>null</c> ni estar compuesto únicamente por espacios en blanco.</param>
    /// <param name="name">Nombre descriptivo del tipo de conocimiento. No puede ser
    /// <c>null</c> ni estar compuesto únicamente por espacios en blanco.</param>
    /// <exception cref="ArgumentException">
    /// Se lanza cuando <paramref name="code"/> o <paramref name="name"/> son <c>null</c>,
    /// están vacíos o contienen únicamente espacios en blanco.
    /// </exception>
    public KnowledgeType(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Code = code;
        Name = name;
    }

    /// <summary>
    /// Actualiza el código y el nombre del tipo de conocimiento.
    /// </summary>
    /// <remarks>
    /// Este método modifica ambas propiedades simultáneamente, validando que los valores
    /// proporcionados no sean nulos ni estén compuestos únicamente por espacios en blanco.
    /// </remarks>
    /// <param name="code">Nuevo código alfanumérico único del tipo de conocimiento. No puede
    /// ser <c>null</c> ni estar compuesto únicamente por espacios en blanco.</param>
    /// <param name="name">Nuevo nombre descriptivo del tipo de conocimiento. No puede ser
    /// <c>null</c> ni estar compuesto únicamente por espacios en blanco.</param>
    /// <exception cref="ArgumentException">
    /// Se lanza cuando <paramref name="code"/> o <paramref name="name"/> son <c>null</c>,
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
