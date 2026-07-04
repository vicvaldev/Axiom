using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record UpdateUserCommand(
    Guid Id,
    string Email,
    string Name) : IRequest<User?>;
