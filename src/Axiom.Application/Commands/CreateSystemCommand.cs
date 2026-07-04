using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
///     Comando CQRS para crear un nuevo sistema en el repositorio de conocimiento.
/// </summary>
/// <param name="EAI">Código identificador único del sistema (máximo 20 caracteres). Ejemplo: "SAP-FI-001".</param>
/// <param name="Name">Nombre descriptivo del sistema (máximo 200 caracteres).</param>
/// <param name="OwnerUserId">Identificador único del usuario propietario del sistema, responsable del mismo.</param>
/// <returns>La entidad <see cref="AxiomSystem"/> creada, incluyendo su identificador numérico asignado.</returns>
public record CreateSystemCommand(
    string EAI,
    string Name,
    Guid OwnerUserId) : IRequest<AxiomSystem>;
