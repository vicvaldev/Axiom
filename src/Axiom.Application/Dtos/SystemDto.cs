namespace Axiom.Application.Dtos;

/// <summary>
/// DTO de proyección para un sistema. Representa la información resumida de un
/// sistema junto con el nombre de su propietario, sin exponer navegaciones completas.
/// </summary>
public class SystemDto
{
    /// <summary>
    /// Identificador único del sistema (clave primaria, identidad auto-generada).
    /// </summary>
    public long SystemId { get; init; }

    /// <summary>
    /// Código EAI del sistema. Longitud máxima de 20 caracteres.
    /// </summary>
    public string EAI { get; init; } = null!;

    /// <summary>
    /// Nombre descriptivo del sistema. Longitud máxima de 200 caracteres.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Identificador del usuario propietario del sistema.
    /// </summary>
    public Guid OwnerUserId { get; init; }

    /// <summary>
    /// Nombre del usuario propietario del sistema.
    /// </summary>
    public string OwnerName { get; init; } = null!;
}
