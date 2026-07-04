using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record CreateSystemCommand(
    string EAI,
    string Name,
    Guid OwnerUserId) : IRequest<AxiomSystem>;
