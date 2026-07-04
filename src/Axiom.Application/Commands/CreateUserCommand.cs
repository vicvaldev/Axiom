using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
///     Comando CQRS para crear un nuevo usuario en el sistema.
/// </summary>
/// <param name="Email">Dirección de correo electrónico del usuario. Debe ser única en el sistema.</param>
/// <param name="Name">Nombre completo del usuario.</param>
/// <returns>La entidad <see cref="User"/> creada, incluyendo su identificador único asignado.</returns>
public record CreateUserCommand(
    string Email,
    string Name) : IRequest<User>;
