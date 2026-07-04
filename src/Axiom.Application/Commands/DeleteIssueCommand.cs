using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
/// Comando para eliminar un incidente o ticket del sistema.
/// </summary>
/// <remarks>
/// Este comando solicita la eliminación de un issue identificado por su <see cref="Guid"/> único.
/// El manejador debe validar la existencia del issue y gestionar las relaciones con conocimientos asociados
/// (por ejemplo, establecer la referencia a <c>null</c> en los conocimientos vinculados) antes de eliminarlo.
/// </remarks>
/// <param name="Id">Identificador único del issue a eliminar.</param>
/// <returns><c>true</c> si el issue se eliminó correctamente; <c>false</c> en caso contrario.</returns>
public record DeleteIssueCommand(Guid Id) : IRequest<bool>;
