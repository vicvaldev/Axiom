using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record CreateSystemComponentCommand(
    long SystemId,
    Guid ComponentId,
    bool IsOwner,
    string? RoleDescription = null) : IRequest<SystemComponent>;
