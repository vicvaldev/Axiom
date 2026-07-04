using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
/// Comando para eliminar una etiqueta de conocimiento.
/// </summary>
/// <remarks>
/// Este comando solicita la eliminación de una etiqueta identificada por su <see cref="long"/> como clave primaria.
/// El manejador debe verificar que la etiqueta no esté asociada a ningún conocimiento antes de eliminarla,
/// o bien gestionar la desvinculación de la tabla intermedia <c>KnowledgeKnowledgeTags</c>.
/// </remarks>
/// <param name="Id">Identificador numérico de la etiqueta a eliminar.</param>
/// <returns><c>true</c> si la etiqueta se eliminó correctamente; <c>false</c> en caso contrario.</returns>
public record DeleteKnowledgeTagCommand(long Id) : IRequest<bool>;
