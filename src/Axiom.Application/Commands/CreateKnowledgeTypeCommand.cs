using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
///     Comando CQRS para crear un nuevo tipo de conocimiento.
///     Los tipos de conocimiento clasifican los artículos (ej: documentación, troubleshooting, guía de usuario).
/// </summary>
/// <param name="Code">Código único del tipo de conocimiento. Ejemplo: "DOC", "TROUBLESHOOT", "GUIDE".</param>
/// <param name="Name">Nombre descriptivo del tipo de conocimiento (máximo 200 caracteres).</param>
/// <returns>La entidad <see cref="KnowledgeType"/> creada, incluyendo su identificador numérico asignado.</returns>
public record CreateKnowledgeTypeCommand(
    string Code,
    string Name) : IRequest<KnowledgeType>;
