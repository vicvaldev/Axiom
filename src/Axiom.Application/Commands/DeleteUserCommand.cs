using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
/// Comando para eliminar un usuario del sistema.
/// </summary>
/// <remarks>
/// Este comando solicita la eliminación lógica o física de un usuario identificado por su <see cref="Guid"/> único.
/// El manejador correspondiente debe validar la existencia del usuario antes de proceder con la eliminación.
/// </remarks>
/// <param name="Id">Identificador único del usuario a eliminar.</param>
/// <returns><c>true</c> si el usuario se eliminó correctamente; <c>false</c> en caso contrario.</returns>
public record DeleteUserCommand(Guid Id) : IRequest<bool>;
