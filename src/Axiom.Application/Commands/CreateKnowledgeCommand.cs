using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
///     Comando CQRS para crear un nuevo artículo de conocimiento en el repositorio.
/// </summary>
/// <param name="Title">Título del artículo de conocimiento.</param>
/// <param name="Summary">Resumen o extracto breve del contenido del artículo.</param>
/// <param name="Content">Cuerpo completo del artículo de conocimiento.</param>
/// <param name="SystemId">Identificador del sistema al que pertenece el conocimiento.</param>
/// <param name="CreatedByUserId">Identificador del usuario que crea el artículo.</param>
/// <param name="KnowledgeTypeId">Identificador del tipo de conocimiento (documentación, troubleshooting, guía, etc.).</param>
/// <param name="KnowledgeStateId">Identificador del estado inicial del conocimiento (borrador, publicado, etc.).</param>
/// <param name="IssueId">Identificador opcional del issue asociado al conocimiento. <c>null</c> si no está vinculado a ningún issue.</param>
/// <param name="Tags">Lista de etiquetas (tags) que categorizan el conocimiento.</param>
/// <param name="KnowledgeId">Identificador opcional para asignar manualmente al conocimiento. Si se omite, se genera automáticamente.</param>
/// <returns>La entidad <see cref="Knowledge"/> creada, incluyendo su identificador único asignado.</returns>
public record CreateKnowledgeCommand(
    string Title,
    string Summary,
    string Content,
    long SystemId,
    Guid CreatedByUserId,
    long KnowledgeTypeId,
    int KnowledgeStateId,
    Guid? IssueId,
    List<string> Tags,
    Guid? KnowledgeId = null) : IRequest<Knowledge>;
