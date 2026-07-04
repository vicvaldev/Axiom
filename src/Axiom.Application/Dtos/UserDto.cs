namespace Axiom.Application.Dtos;

/// <summary>
/// DTO de proyección para un usuario. Contiene los datos básicos de un usuario
/// del sistema, sin incluir navegaciones a otras entidades.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Dirección de correo electrónico del usuario. Es única en el sistema.
    /// </summary>
    public string Email { get; init; } = null!;

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public string Name { get; init; } = null!;
}
