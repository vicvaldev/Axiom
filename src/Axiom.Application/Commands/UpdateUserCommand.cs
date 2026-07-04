using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
///     Comando para actualizar los datos de un usuario existente en el sistema.
///     Implementa <see cref="IRequest{TResponse}" /> con <see cref="User" /> como respuesta,
///     devolviendo la entidad actualizada o <c>null</c> si el usuario no se encuentra.
/// </summary>
/// <param name="Id">Identificador único del usuario que se desea actualizar.</param>
/// <param name="Email">Nueva dirección de correo electrónico del usuario.</param>
/// <param name="Name">Nuevo nombre completo del usuario.</param>
/// <returns>La entidad <see cref="User" /> actualizada, o <c>null</c> si no existe ningún usuario con el <paramref name="Id" /> especificado.</returns>
public record UpdateUserCommand(
    Guid Id,
    string Email,
    string Name) : IRequest<User?>;
