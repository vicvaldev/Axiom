using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
/// Comando para eliminar un estado de issue del catálogo de estados de incidentes.
/// </summary>
/// <remarks>
/// Este comando solicita la eliminación de un estado de issue identificado por su <see cref="int"/> como clave primaria.
/// El manejador debe verificar que ningún issue activo esté utilizando este estado antes de eliminarlo,
/// o bien reasignar los issues a un estado por defecto.
/// </remarks>
/// <param name="Id">Identificador numérico del estado de issue a eliminar.</param>
/// <returns><c>true</c> si el estado de issue se eliminó correctamente; <c>false</c> en caso contrario.</returns>
public record DeleteIssueStateCommand(int Id) : IRequest<bool>;
