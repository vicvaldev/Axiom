using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
///     Comando para actualizar los datos de un sistema existente en el catálogo de sistemas.
///     Implementa <see cref="IRequest{TResponse}" /> con <see cref="AxiomSystem" /> como respuesta,
///     devolviendo la entidad actualizada o <c>null</c> si el sistema no se encuentra.
/// </summary>
/// <param name="Id">Identificador único del sistema que se desea actualizar.</param>
/// <param name="EAI">Nuevo código EAI (hasta 20 caracteres) que identifica al sistema.</param>
/// <param name="Name">Nuevo nombre descriptivo del sistema (hasta 200 caracteres).</param>
/// <param name="OwnerUserId">Identificador del usuario propietario del sistema.</param>
/// <returns>La entidad <see cref="AxiomSystem" /> actualizada, o <c>null</c> si no existe ningún sistema con el <paramref name="Id" /> especificado.</returns>
public record UpdateSystemCommand(
    long Id,
    string EAI,
    string Name,
    Guid OwnerUserId) : IRequest<AxiomSystem?>;
