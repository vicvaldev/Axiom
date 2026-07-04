using Axiom.Application.Dtos;
using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

/// <summary>
/// Servicio de inicialización de la aplicación.
/// Proporciona operaciones para crear los datos maestros esenciales (usuarios,
/// sistemas, tipos de conocimiento, estados) y para sembrar datos de demostración
/// que faciliten las pruebas y la exploración del sistema.
/// </summary>
public interface IStartupService
{
    /// <summary>
    /// Crea un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="email">Dirección de correo electrónico única del usuario.</param>
    /// <param name="name">Nombre completo del usuario.</param>
    /// <param name="ct">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="User"/> recién creada.</returns>
    Task<User> CreateUserAsync(string email, string name, CancellationToken ct);

    /// <summary>
    /// Crea un nuevo sistema en el repositorio.
    /// </summary>
    /// <param name="eai">Código EAI único que identifica al sistema (máx. 20 caracteres).</param>
    /// <param name="name">Nombre descriptivo del sistema (máx. 200 caracteres).</param>
    /// <param name="ownerUserId">Identificador único del usuario propietario del sistema.</param>
    /// <param name="ct">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="AxiomSystem"/> recién creada.</returns>
    Task<AxiomSystem> CreateSystemAsync(string eai, string name, Guid ownerUserId, CancellationToken ct);

    /// <summary>
    /// Crea un nuevo tipo de conocimiento en el catálogo.
    /// </summary>
    /// <param name="code">Código único del tipo de conocimiento.</param>
    /// <param name="name">Nombre descriptivo del tipo de conocimiento (máx. 200 caracteres).</param>
    /// <param name="ct">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="KnowledgeType"/> recién creada.</returns>
    Task<KnowledgeType> CreateKnowledgeTypeAsync(string code, string name, CancellationToken ct);

    /// <summary>
    /// Crea un nuevo estado de incidencia en el catálogo.
    /// </summary>
    /// <param name="code">Código único del estado de incidencia.</param>
    /// <param name="name">Nombre descriptivo del estado de incidencia (máx. 200 caracteres).</param>
    /// <param name="ct">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="IssueState"/> recién creada.</returns>
    Task<IssueState> CreateIssueStateAsync(string code, string name, CancellationToken ct);

    /// <summary>
    /// Crea un nuevo estado de conocimiento en el catálogo.
    /// </summary>
    /// <param name="code">Código único del estado de conocimiento.</param>
    /// <param name="name">Nombre descriptivo del estado de conocimiento (máx. 200 caracteres).</param>
    /// <param name="ct">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="KnowledgeState"/> recién creada.</returns>
    Task<KnowledgeState> CreateKnowledgeStateAsync(string code, string name, CancellationToken ct);

    /// <summary>
    /// Siembra datos de demostración en el sistema para fines de prueba y exploración.
    /// </summary>
    /// <param name="ct">Token de cancelación para la operación asíncrona.</param>
    /// <returns>Un objeto <see cref="DemoSeedResultDto"/> con el resumen de los datos creados.</returns>
    Task<DemoSeedResultDto> SeedDemoDataAsync(CancellationToken ct);
}
