using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record UpdateSystemCommand(
    long Id,
    string EAI,
    string Name,
    Guid OwnerUserId) : IRequest<AxiomSystem?>;
