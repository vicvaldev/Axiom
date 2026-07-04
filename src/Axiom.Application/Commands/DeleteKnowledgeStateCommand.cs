using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
/// Comando para eliminar un estado de conocimiento del catálogo de estados.
/// </summary>
/// <remarks>
/// Este comando solicita la eliminación de un estado de conocimiento identificado por su <see cref="int"/> como clave primaria.
/// El manejador debe asegurarse de que ningún conocimiento haga referencia a este estado antes de eliminarlo,
/// o bien reasignar los conocimientos a un estado por defecto.
/// </remarks>
/// <param name="Id">Identificador numérico del estado de conocimiento a eliminar.</param>
/// <returns><c>true</c> si el estado de conocimiento se eliminó correctamente; <c>false</c> en caso contrario.</returns>
public record DeleteKnowledgeStateCommand(int Id) : IRequest<bool>;
