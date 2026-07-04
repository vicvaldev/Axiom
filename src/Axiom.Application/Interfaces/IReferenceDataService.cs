using Axiom.Application.Dtos;

namespace Axiom.Application.Interfaces;

/// <summary>
/// Servicio de acceso a datos de referencia (catálogos maestros) de la aplicación.
/// Proporciona operaciones de consulta para listar y buscar entidades base como
/// usuarios, sistemas, tipos de conocimiento, estados de conocimiento y estados
/// de incidencias.
/// </summary>
public interface IReferenceDataService
{
    /// <summary>
    /// Obtiene la lista completa de usuarios disponibles en el sistema.
    /// </summary>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Lista de lecturas de objetos <see cref="UserDto"/> representando los usuarios.</returns>
    Task<IReadOnlyList<UserDto>> ListUsersAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene la lista completa de sistemas registrados.
    /// </summary>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Lista de lecturas de objetos <see cref="SystemDto"/> representando los sistemas.</returns>
    Task<IReadOnlyList<SystemDto>> ListSystemsAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene la lista completa de tipos de conocimiento disponibles.
    /// </summary>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Lista de lecturas de objetos <see cref="ReferenceCodeDto"/> representando los tipos de conocimiento.</returns>
    Task<IReadOnlyList<ReferenceCodeDto>> ListKnowledgeTypesAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene la lista completa de estados de conocimiento disponibles.
    /// </summary>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Lista de lecturas de objetos <see cref="ReferenceCodeDto"/> representando los estados de conocimiento.</returns>
    Task<IReadOnlyList<ReferenceCodeDto>> ListKnowledgeStatesAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene la lista completa de estados de incidencias disponibles.
    /// </summary>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Lista de lecturas de objetos <see cref="ReferenceCodeDto"/> representando los estados de incidencias.</returns>
    Task<IReadOnlyList<ReferenceCodeDto>> ListIssueStatesAsync(CancellationToken ct = default);

    /// <summary>
    /// Busca un usuario por su dirección de correo electrónico.
    /// </summary>
    /// <param name="email">Dirección de correo electrónico del usuario a buscar.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>El objeto <see cref="UserDto"/> si se encuentra; de lo contrario, <c>null</c>.</returns>
    Task<UserDto?> FindUserByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Busca un sistema por su código EAI (identificador único del sistema).
    /// </summary>
    /// <param name="eai">Código EAI del sistema a buscar.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>El objeto <see cref="SystemDto"/> si se encuentra; de lo contrario, <c>null</c>.</returns>
    Task<SystemDto?> FindSystemByEaiAsync(string eai, CancellationToken ct = default);

    /// <summary>
    /// Busca un tipo de conocimiento por su código único.
    /// </summary>
    /// <param name="code">Código único del tipo de conocimiento a buscar.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>El objeto <see cref="ReferenceCodeDto"/> si se encuentra; de lo contrario, <c>null</c>.</returns>
    Task<ReferenceCodeDto?> FindKnowledgeTypeByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Busca un estado de conocimiento por su código único.
    /// </summary>
    /// <param name="code">Código único del estado de conocimiento a buscar.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>El objeto <see cref="ReferenceCodeDto"/> si se encuentra; de lo contrario, <c>null</c>.</returns>
    Task<ReferenceCodeDto?> FindKnowledgeStateByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Busca un estado de incidencia por su código único.
    /// </summary>
    /// <param name="code">Código único del estado de incidencia a buscar.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>El objeto <see cref="ReferenceCodeDto"/> si se encuentra; de lo contrario, <c>null</c>.</returns>
    Task<ReferenceCodeDto?> FindIssueStateByCodeAsync(string code, CancellationToken ct = default);
}
