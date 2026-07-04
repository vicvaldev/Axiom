namespace Axiom.Domain.Entities;

/// <summary>
/// Representa una incidencia o solicitud de TI dentro del sistema Axiom.
/// Cada <see cref="Issue"/> está asociado a un <see cref="AxiomSystem"/> y a un <see cref="User"/> creador,
/// y posee un ciclo de vida controlado por <see cref="IssueState"/>.
/// Contiene información descriptiva (resumen, problema, análisis, resolución) y opcionalmente
/// números de referencia externa como RITM o incidente.
/// Una incidencia puede estar vinculada a varios <see cref="Knowledge"/> que documentan su solución.
/// </summary>
public class Issue
{
    /// <summary>
    /// Identificador único de la incidencia. Se asigna al crear la entidad.
    /// </summary>
    public Guid IssueId { get; private set; }

    /// <summary>
    /// Resumen breve y descriptivo de la incidencia. No puede ser nulo ni estar vacío.
    /// </summary>
    public string Summary { get; private set; } = null!;

    /// <summary>
    /// Número de solicitud RITM (Request Item) asociado desde un sistema externo de gestión de servicios.
    /// Puede ser <c>null</c> si no aplica.
    /// </summary>
    public string? RitmNumber { get; private set; }

    /// <summary>
    /// Número de incidente asociado desde un sistema externo de gestión de incidencias.
    /// Puede ser <c>null</c> si no aplica.
    /// </summary>
    public string? IncidentNumber { get; private set; }

    /// <summary>
    /// Identificador del sistema <see cref="AxiomSystem"/> al que pertenece esta incidencia.
    /// </summary>
    public long SystemId { get; private set; }

    /// <summary>
    /// Descripción detallada del problema detectado. No puede ser nulo ni estar vacío.
    /// </summary>
    public string Problem { get; private set; } = null!;

    /// <summary>
    /// Análisis técnico realizado para diagnosticar la causa del problema.
    /// Puede ser una cadena vacía si aún no se ha analizado.
    /// </summary>
    public string Analysis { get; private set; } = null!;

    /// <summary>
    /// Descripción de la resolución aplicada para solucionar la incidencia.
    /// Se establece cuando se resuelve la incidencia mediante <see cref="Resolve"/>.
    /// </summary>
    public string Resolution { get; private set; } = null!;

    /// <summary>
    /// Identificador del estado actual de la incidencia (<see cref="IssueState"/>).
    /// Controla el ciclo de vida: abierta, en análisis, resuelta, cerrada, etc.
    /// </summary>
    public int StateId { get; private set; }

    /// <summary>
    /// Identificador del usuario (<see cref="User"/>) que creó la incidencia.
    /// </summary>
    public Guid CreatedByUserId { get; private set; }

    /// <summary>
    /// Fecha y hora (UTC) en que se creó la incidencia.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Fecha y hora (UTC) de la última modificación realizada sobre la incidencia.
    /// Se actualiza automáticamente al modificar cualquier propiedad.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Fecha y hora (UTC) en que la incidencia fue resuelta.
    /// Es <c>null</c> mientras la incidencia no esté resuelta.
    /// Se establece al invocar <see cref="Resolve"/>.
    /// </summary>
    public DateTime? ResolvedAt { get; private set; }

    /// <summary>
    /// Navegación al sistema <see cref="AxiomSystem"/> asociado.
    /// </summary>
    public AxiomSystem System { get; private set; } = null!;

    /// <summary>
    /// Navegación al estado <see cref="IssueState"/> actual de la incidencia.
    /// </summary>
    public IssueState State { get; private set; } = null!;

    /// <summary>
    /// Navegación al usuario <see cref="User"/> que creó la incidencia.
    /// </summary>
    public User CreatedBy { get; private set; } = null!;

    /// <summary>
    /// Colección de conocimientos <see cref="Knowledge"/> asociados a esta incidencia.
    /// Representa los artículos que documentan la solución o el análisis del problema.
    /// </summary>
    public ICollection<Knowledge> Knowledges { get; private set; } = new HashSet<Knowledge>();

    /// <summary>
    /// Constructor privado sin parámetros requerido por Entity Framework Core
    /// para la creación de proxies y la hidratación de entidades desde la base de datos.
    /// </summary>
    private Issue() { }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="Issue"/> con los datos esenciales.
    /// Establece <see cref="CreatedAt"/> y <see cref="UpdatedAt"/> con la fecha y hora UTC actuales.
    /// Si no se proporciona un identificador, se genera automáticamente un nuevo <see cref="Guid"/>.
    /// </summary>
    /// <param name="summary">Resumen de la incidencia. No puede ser nulo ni estar compuesto solo por espacios en blanco.</param>
    /// <param name="systemId">Identificador del sistema <see cref="AxiomSystem"/> al que pertenece la incidencia.</param>
    /// <param name="problem">Descripción detallada del problema. No puede ser nulo ni estar compuesto solo por espacios en blanco.</param>
    /// <param name="stateId">Identificador del estado inicial <see cref="IssueState"/> de la incidencia.</param>
    /// <param name="createdByUserId">Identificador del usuario <see cref="User"/> que crea la incidencia.</param>
    /// <param name="analysis">Análisis técnico inicial. Si es <c>null</c>, se asigna una cadena vacía.</param>
    /// <param name="resolution">Resolución inicial. Si es <c>null</c>, se asigna una cadena vacía.</param>
    /// <param name="ritmNumber">Número de solicitud RITM asociado (opcional).</param>
    /// <param name="incidentNumber">Número de incidente asociado (opcional).</param>
    /// <param name="issueId">Identificador único de la incidencia. Si es <c>null</c>, se genera automáticamente.</param>
    /// <exception cref="ArgumentException">
    /// Se lanza si <paramref name="summary"/> o <paramref name="problem"/> son <c>null</c>, vacíos o contienen solo espacios en blanco.
    /// </exception>
    public Issue(
        string summary,
        long systemId,
        string problem,
        int stateId,
        Guid createdByUserId,
        string? analysis = null,
        string? resolution = null,
        string? ritmNumber = null,
        string? incidentNumber = null,
        Guid? issueId = null)
    {
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Summary cannot be empty.", nameof(summary));
        if (string.IsNullOrWhiteSpace(problem))
            throw new ArgumentException("Problem cannot be empty.", nameof(problem));

        IssueId = issueId ?? Guid.NewGuid();
        Summary = summary;
        SystemId = systemId;
        Problem = problem;
        Analysis = analysis ?? string.Empty;
        Resolution = resolution ?? string.Empty;
        StateId = stateId;
        CreatedByUserId = createdByUserId;
        RitmNumber = ritmNumber;
        IncidentNumber = incidentNumber;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Resuelve la incidencia estableciendo el estado de resolución, la descripción de la solución
    /// y la fecha de resolución (<see cref="ResolvedAt"/>).
    /// Actualiza también <see cref="UpdatedAt"/> con la fecha y hora UTC actuales.
    /// </summary>
    /// <param name="stateId">Identificador del estado <see cref="IssueState"/> que indica la resolución (por ejemplo, "Resuelta" o "Cerrada").</param>
    /// <param name="resolution">Descripción detallada de la resolución aplicada. Si es <c>null</c>, se asigna una cadena vacía.</param>
    public void Resolve(int stateId, string resolution)
    {
        StateId = stateId;
        Resolution = resolution ?? string.Empty;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza únicamente el estado de la incidencia (<see cref="StateId"/>).
    /// Útil para transiciones de estado que no requieren modificar otros campos.
    /// Actualiza <see cref="UpdatedAt"/> con la fecha y hora UTC actuales.
    /// </summary>
    /// <param name="stateId">Nuevo identificador del estado <see cref="IssueState"/>.</param>
    public void UpdateState(int stateId)
    {
        StateId = stateId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza todos los campos editables de la incidencia con nueva información.
    /// Este método reemplaza los valores actuales de resumen, problema, análisis, resolución,
    /// sistema, estado, RITM y número de incidente.
    /// Actualiza <see cref="UpdatedAt"/> con la fecha y hora UTC actuales.
    /// </summary>
    /// <param name="summary">Nuevo resumen de la incidencia. No puede ser nulo ni estar compuesto solo por espacios en blanco.</param>
    /// <param name="problem">Nueva descripción del problema. No puede ser nulo ni estar compuesto solo por espacios en blanco.</param>
    /// <param name="analysis">Nuevo análisis técnico. Si es <c>null</c>, se asigna una cadena vacía.</param>
    /// <param name="resolution">Nueva resolución. Si es <c>null</c>, se asigna una cadena vacía.</param>
    /// <param name="systemId">Nuevo identificador del sistema <see cref="AxiomSystem"/> asociado.</param>
    /// <param name="stateId">Nuevo identificador del estado <see cref="IssueState"/>.</param>
    /// <param name="ritmNumber">Nuevo número de solicitud RITM (opcional).</param>
    /// <param name="incidentNumber">Nuevo número de incidente (opcional).</param>
    /// <exception cref="ArgumentException">
    /// Se lanza si <paramref name="summary"/> o <paramref name="problem"/> son <c>null</c>, vacíos o contienen solo espacios en blanco.
    /// </exception>
    public void Update(
        string summary,
        string problem,
        string? analysis,
        string? resolution,
        long systemId,
        int stateId,
        string? ritmNumber,
        string? incidentNumber)
    {
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Summary cannot be empty.", nameof(summary));
        if (string.IsNullOrWhiteSpace(problem))
            throw new ArgumentException("Problem cannot be empty.", nameof(problem));

        Summary = summary;
        Problem = problem;
        Analysis = analysis ?? string.Empty;
        Resolution = resolution ?? string.Empty;
        SystemId = systemId;
        StateId = stateId;
        RitmNumber = ritmNumber;
        IncidentNumber = incidentNumber;
        UpdatedAt = DateTime.UtcNow;
    }
}
