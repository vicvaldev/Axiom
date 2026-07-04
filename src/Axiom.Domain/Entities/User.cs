namespace Axiom.Domain.Entities;

/// <summary>
/// Representa un usuario del sistema dentro del dominio Axiom.
/// Actúa como raíz del agregado de usuarios y es la entidad central para la propiedad de sistemas,
/// así como para la creación de incidencias y conocimientos.
/// </summary>
/// <remarks>
/// La entidad <see cref="User"/> se identifica de forma única mediante un <see cref="Guid"/> generado
/// al momento de su creación. Garantiza que el correo electrónico y el nombre no sean nulos ni estén
/// vacíos mediante validaciones en el constructor y en el método <see cref="Update"/>.
/// Las colecciones de navegación (<see cref="OwnedSystems"/>, <see cref="CreatedIssues"/>,
/// <see cref="CreatedKnowledges"/>) se inicializan como <see cref="HashSet{T}"/> para evitar
/// duplicados y no admiten carga diferida (<c>lazy loading</c>).
/// </remarks>
public class User
{
    /// <summary>
    /// Obtiene el identificador único del usuario.
    /// </summary>
    /// <value>Un <see cref="Guid"/> generado automáticamente al crear la entidad.</value>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Obtiene la dirección de correo electrónico del usuario.
    /// </summary>
    /// <value>Una cadena no nula ni vacía que representa el correo electrónico. Se almacena como única en la base de datos.</value>
    public string Email { get; private set; } = null!;

    /// <summary>
    /// Obtiene el nombre completo del usuario.
    /// </summary>
    /// <value>Una cadena no nula ni vacía con el nombre del usuario.</value>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Obtiene la colección de sistemas propiedad del usuario.
    /// </summary>
    /// <value>Conjunto de tipo <see cref="HashSet{AxiomSystem}"/> que contiene los sistemas que este usuario posee.</value>
    public ICollection<AxiomSystem> OwnedSystems { get; private set; } = new HashSet<AxiomSystem>();

    /// <summary>
    /// Obtiene la colección de incidencias creadas por el usuario.
    /// </summary>
    /// <value>Conjunto de tipo <see cref="HashSet{Issue}"/> que contiene las incidencias asociadas a este usuario como creador.</value>
    public ICollection<Issue> CreatedIssues { get; private set; } = new HashSet<Issue>();

    /// <summary>
    /// Obtiene la colección de conocimientos creados por el usuario.
    /// </summary>
    /// <value>Conjunto de tipo <see cref="HashSet{Knowledge}"/> que contiene los conocimientos asociados a este usuario como creador.</value>
    public ICollection<Knowledge> CreatedKnowledges { get; private set; } = new HashSet<Knowledge>();

    /// <summary>
    /// Constructor privado sin parámetros requerido por Entity Framework Core para la creación de proxies
    /// y la reconstrucción de entidades desde la base de datos.
    /// </summary>
    /// <remarks>
    /// No debe ser utilizado directamente desde el código de aplicación.
    /// </remarks>
    private User() { }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="User"/> con un correo electrónico y un nombre.
    /// </summary>
    /// <param name="email">Dirección de correo electrónico del usuario. No puede ser <c>null</c> ni estar compuesta únicamente por espacios en blanco.</param>
    /// <param name="name">Nombre completo del usuario. No puede ser <c>null</c> ni estar compuesto únicamente por espacios en blanco.</param>
    /// <exception cref="ArgumentException">
    /// Se produce cuando <paramref name="email"/> o <paramref name="name"/> son <c>null</c>, vacíos o contienen solo espacios en blanco.
    /// </exception>
    public User(string email, string name)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        UserId = Guid.NewGuid();
        Email = email;
        Name = name;
    }

    /// <summary>
    /// Actualiza el correo electrónico y el nombre del usuario.
    /// </summary>
    /// <param name="email">Nueva dirección de correo electrónico. No puede ser <c>null</c> ni estar compuesta únicamente por espacios en blanco.</param>
    /// <param name="name">Nuevo nombre completo. No puede ser <c>null</c> ni estar compuesto únicamente por espacios en blanco.</param>
    /// <exception cref="ArgumentException">
    /// Se produce cuando <paramref name="email"/> o <paramref name="name"/> son <c>null</c>, vacíos o contienen solo espacios en blanco.
    /// </exception>
    /// <remarks>
    /// Este método modifica las propiedades <see cref="Email"/> y <see cref="Name"/> de la instancia actual.
    /// No altera las colecciones de navegación ni el identificador <see cref="UserId"/>.
    /// </remarks>
    public void Update(string email, string name)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Email = email;
        Name = name;
    }
}
