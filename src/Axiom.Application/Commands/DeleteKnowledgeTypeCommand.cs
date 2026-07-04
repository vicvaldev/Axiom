using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
/// Comando para eliminar un tipo de conocimiento del catálogo de tipos.
/// </summary>
/// <remarks>
/// Este comando solicita la eliminación de un tipo de conocimiento identificado por su <see cref="long"/> como clave primaria.
/// El manejador debe validar que el tipo no esté siendo utilizado por ningún conocimiento activo antes de eliminarlo.
/// </remarks>
/// <param name="Id">Identificador numérico del tipo de conocimiento a eliminar.</param>
/// <returns><c>true</c> si el tipo de conocimiento se eliminó correctamente; <c>false</c> en caso contrario.</returns>
public record DeleteKnowledgeTypeCommand(long Id) : IRequest<bool>;
