using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
///     Comando para actualizar un artículo de conocimiento existente en la base de conocimiento.
///     Implementa <see cref="IRequest{TResponse}" /> con <see cref="Knowledge" /> como respuesta,
///     devolviendo la entidad actualizada o <c>null</c> si el conocimiento no se encuentra.
/// </summary>
/// <param name="Id">Identificador único del artículo de conocimiento que se desea actualizar.</param>
/// <param name="Title">Nuevo título del artículo de conocimiento.</param>
/// <param name="Summary">Nuevo resumen o extracto breve del contenido del artículo de conocimiento.</param>
/// <param name="Content">Nuevo contenido completo del artículo de conocimiento.</param>
/// <param name="SystemId">Identificador del sistema al que pertenece el artículo de conocimiento.</param>
/// <param name="KnowledgeTypeId">Identificador del tipo de conocimiento asociado al artículo.</param>
/// <param name="KnowledgeStateId">Identificador del estado actual del conocimiento.</param>
/// <param name="IssueId">Identificador opcional del incidente o solicitud (RITM) relacionado con el artículo de conocimiento. Puede ser <c>null</c> si no existe vinculación.</param>
/// <param name="Tags">Lista de etiquetas (nombres) que se asignarán al artículo de conocimiento, reemplazando cualquier conjunto anterior de etiquetas.</param>
/// <returns>La entidad <see cref="Knowledge" /> actualizada, o <c>null</c> si no existe ningún artículo de conocimiento con el <paramref name="Id" /> especificado.</returns>
public record UpdateKnowledgeCommand(
    Guid Id,
    string Title,
    string Summary,
    string Content,
    long SystemId,
    long KnowledgeTypeId,
    int KnowledgeStateId,
    Guid? IssueId,
    List<string> Tags) : IRequest<Knowledge?>;
