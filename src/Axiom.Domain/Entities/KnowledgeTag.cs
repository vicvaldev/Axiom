namespace Axiom.Domain.Entities;

/// <summary>
/// Representa una etiqueta o tag utilizada para categorizar y filtrar conocimientos
/// (<see cref="Knowledge"/>) dentro del sistema.
/// </summary>
/// <remarks>
/// La entidad <see cref="KnowledgeTag"/> mantiene una relación muchos a muchos con
/// <see cref="Knowledge"/> a través de la tabla intermedia
/// <see cref="KnowledgeKnowledgeTag"/>. Cada etiqueta tiene un nombre único
/// (<see cref="TagName"/>) que la identifica de forma descriptiva.
/// </remarks>
public class KnowledgeTag
{
    /// <summary>
    /// Identificador único autogenerado de la etiqueta (clave primaria, identity).
    /// </summary>
    public long KnowledgeTagId { get; private set; }

    /// <summary>
    /// Nombre descriptivo de la etiqueta. Debe ser un texto no vacío ni compuesto
    /// únicamente por espacios en blanco.
    /// </summary>
    public string TagName { get; private set; } = null!;

    /// <summary>
    /// Colección de relaciones muchos a muchos entre esta etiqueta y las entidades
    /// <see cref="Knowledge"/> asociadas.
    /// </summary>
    /// <remarks>
    /// Se inicializa como un <see cref="HashSet{T}"/> para evitar duplicados y
    /// garantizar un rendimiento óptimo en las operaciones de agregado y eliminación.
    /// Esta propiedad es de solo lectura desde el exterior; las relaciones se
    /// gestionan a través del constructor o métodos específicos del agregado.
    /// </remarks>
    public ICollection<KnowledgeKnowledgeTag> KnowledgeKnowledgeTags { get; private set; } = new HashSet<KnowledgeKnowledgeTag>();

    /// <summary>
    /// Constructor privado sin parámetros requerido por Entity Framework Core
    /// para la creación de proxies y la hidratación de entidades desde la base de datos.
    /// </summary>
    /// <remarks>
    /// No debe ser utilizado directamente desde el código de la aplicación.
    /// Utilice el constructor público <see cref="KnowledgeTag(string)"/> para
    /// crear nuevas instancias.
    /// </remarks>
    private KnowledgeTag() { }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="KnowledgeTag"/>
    /// con el nombre de etiqueta especificado.
    /// </summary>
    /// <param name="tagName">Nombre de la etiqueta. No puede ser <c>null</c>,
    /// vacío ni estar compuesto únicamente por espacios en blanco.</param>
    /// <exception cref="ArgumentException">
    /// Se produce cuando <paramref name="tagName"/> es <c>null</c>, vacío o
    /// contiene únicamente espacios en blanco.
    /// </exception>
    public KnowledgeTag(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            throw new ArgumentException("El nombre de la etiqueta no puede estar vacío.", nameof(tagName));

        TagName = tagName;
    }

    /// <summary>
    /// Actualiza el nombre de la etiqueta por uno nuevo, validando que el valor
    /// proporcionado no sea nulo, vacío o contenga únicamente espacios en blanco.
    /// </summary>
    /// <param name="tagName">Nuevo nombre para la etiqueta. No puede ser <c>null</c>,
    /// vacío ni estar compuesto únicamente por espacios en blanco.</param>
    /// <exception cref="ArgumentException">
    /// Se produce cuando <paramref name="tagName"/> es <c>null</c>, vacío o
    /// contiene únicamente espacios en blanco.
    /// </exception>
    /// <remarks>
    /// Este método modifica directamente el valor de <see cref="TagName"/> en la
    /// instancia actual. No persiste los cambios en la base de datos; para ello
    /// debe llamarse al método <c>SaveChanges</c> del contexto de datos.
    /// </remarks>
    public void Update(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            throw new ArgumentException("El nombre de la etiqueta no puede estar vacío.", nameof(tagName));

        TagName = tagName;
    }
}
