using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
/// Comando para eliminar un artículo de conocimiento de la base de conocimiento.
/// </summary>
/// <remarks>
/// Este comando solicita la eliminación de un conocimiento identificado por su <see cref="Guid"/> único.
/// El manejador debe validar la existencia del conocimiento y gestionar las relaciones con etiquetas,
/// tipos, estados y posibles incidencias asociadas antes de proceder con la eliminación.
/// </remarks>
/// <param name="Id">Identificador único del conocimiento a eliminar.</param>
/// <returns><c>true</c> si el conocimiento se eliminó correctamente; <c>false</c> en caso contrario.</returns>
public record DeleteKnowledgeCommand(Guid Id) : IRequest<bool>;
