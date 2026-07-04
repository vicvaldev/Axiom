namespace Axiom.Domain.Entities;

/// <summary>
/// Representa un sistema de TI dentro del dominio. Cada sistema pertenece a un usuario propietario
/// y puede contener incidencias (<see cref="Issue"/>) y conocimientos (<see cref="Knowledge"/>) asociados.
/// </summary>
/// <remarks>
/// Esta entidad es agregada raíz en el contexto de sistemas. Su identidad se basa en
/// <see cref="SystemId"/> (autonumérico generado por la base de datos). Los valores
/// <see cref="EAI"/> y <see cref="Name"/> son obligatorios y se validan en el constructor
/// de dominio y en el método <see cref="Update"/>.
/// </remarks>
public class AxiomSystem
{
    /// <summary>
    /// Identificador único del sistema. Se genera automáticamente como clave primaria
    /// autonumérica (<c>bigint identity</c>) en la base de datos.
    /// </summary>
    /// <value>Valor entero de 64 bits que identifica de forma única al sistema.</value>
    public long SystemId { get; private set; }

    /// <summary>
    /// Código EAI (Enterprise Application Identifier) del sistema.
    /// Es un identificador corto y único dentro de la organización, con una longitud máxima de 20 caracteres.
    /// </summary>
    /// <value>Cadena no vacía que representa el identificador EAI del sistema.</value>
    public string EAI { get; private set; } = null!;

    /// <summary>
    /// Nombre descriptivo del sistema de TI.
    /// </summary>
    /// <value>Cadena no vacía con el nombre del sistema, hasta 200 caracteres.</value>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Identificador del usuario propietario del sistema.
    /// </summary>
    /// <value>Identificador único (<see cref="Guid"/>) del usuario propietario,
    /// correspondiente a la entidad <see cref="User"/>.</value>
    public Guid OwnerUserId { get; private set; }

    /// <summary>
    /// Navegación al usuario propietario del sistema.
    /// </summary>
    /// <value>Instancia de <see cref="User"/> que representa al propietario del sistema.</value>
    public User Owner { get; private set; } = null!;

    /// <summary>
    /// Colección de incidencias (<see cref="Issue"/>) asociadas a este sistema.
    /// Se inicializa como un <see cref="HashSet{T}"/> vacío para evitar valores <c>null</c>.
    /// </summary>
    /// <value>Colección de incidencias pertenecientes al sistema.</value>
    public ICollection<Issue> Issues { get; private set; } = new HashSet<Issue>();

    /// <summary>
    /// Colección de conocimientos (<see cref="Knowledge"/>) asociados a este sistema.
    /// Se inicializa como un <see cref="HashSet{T}"/> vacío para evitar valores <c>null</c>.
    /// </summary>
    /// <value>Colección de conocimientos registrados para el sistema.</value>
    public ICollection<Knowledge> Knowledges { get; private set; } = new HashSet<Knowledge>();

    /// <summary>
    /// Colección de componentes técnicos (<see cref="TechnicalComponent"/>) asociados a este sistema.
    /// Se inicializa como un <see cref="HashSet{T}"/> vacío para evitar valores <c>null</c>.
    /// </summary>
    /// <value>Colección de componentes técnicos pertenecientes al sistema.</value>
    public ICollection<TechnicalComponent> TechnicalComponents { get; private set; } = new HashSet<TechnicalComponent>();

    /// <summary>
    /// Colección de asociaciones (<see cref="SystemComponent"/>) entre este sistema y sus componentes técnicos.
    /// Se inicializa como un <see cref="HashSet{T}"/> vacío para evitar valores <c>null</c>.
    /// </summary>
    /// <value>Colección de asociaciones sistema-componente pertenecientes al sistema.</value>
    public ICollection<SystemComponent> SystemComponents { get; private set; } = new HashSet<SystemComponent>();

    /// <summary>
    /// Constructor privado sin parámetros requerido por Entity Framework Core para la
    /// deserialización de proxies y la creación de instancias desde la base de datos.
    /// </summary>
    /// <remarks>
    /// No debe ser utilizado directamente desde el código de aplicación. Las instancias
    /// válidas deben crearse a través del constructor público que valida los parámetros.
    /// </remarks>
    private AxiomSystem() { }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="AxiomSystem"/> con los valores
    /// obligatorios especificados.
    /// </summary>
    /// <param name="eai">Código EAI del sistema. No puede ser <c>null</c>, vacío ni contener
    /// solo espacios en blanco.</param>
    /// <param name="name">Nombre descriptivo del sistema. No puede ser <c>null</c>, vacío ni
    /// contener solo espacios en blanco.</param>
    /// <param name="ownerUserId">Identificador único (<see cref="Guid"/>) del usuario propietario
    /// del sistema.</param>
    /// <exception cref="ArgumentException">
    /// Se produce cuando <paramref name="eai"/> o <paramref name="name"/> son <c>null</c>,
    /// están vacíos o contienen únicamente espacios en blanco.
    /// </exception>
    public AxiomSystem(string eai, string name, Guid ownerUserId)
    {
        if (string.IsNullOrWhiteSpace(eai))
            throw new ArgumentException("EAI cannot be empty.", nameof(eai));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        EAI = eai;
        Name = name;
        OwnerUserId = ownerUserId;
    }

    /// <summary>
    /// Actualiza los datos del sistema con nueva información.
    /// </summary>
    /// <param name="eai">Nuevo código EAI del sistema. No puede ser <c>null</c>, vacío ni
    /// contener solo espacios en blanco.</param>
    /// <param name="name">Nuevo nombre descriptivo del sistema. No puede ser <c>null</c>,
    /// vacío ni contener solo espacios en blanco.</param>
    /// <param name="ownerUserId">Nuevo identificador (<see cref="Guid"/>) del usuario
    /// propietario del sistema.</param>
    /// <exception cref="ArgumentException">
    /// Se produce cuando <paramref name="eai"/> o <paramref name="name"/> son <c>null</c>,
    /// están vacíos o contienen únicamente espacios en blanco.
    /// </exception>
    /// <remarks>
    /// Este método modifica directamente las propiedades de la instancia actual.
    /// No persiste los cambios en la base de datos; eso debe gestionarse a través del
    /// repositorio o contexto de persistencia correspondiente.
    /// </remarks>
    public void Update(string eai, string name, Guid ownerUserId)
    {
        if (string.IsNullOrWhiteSpace(eai))
            throw new ArgumentException("EAI cannot be empty.", nameof(eai));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        EAI = eai;
        Name = name;
        OwnerUserId = ownerUserId;
    }
}
