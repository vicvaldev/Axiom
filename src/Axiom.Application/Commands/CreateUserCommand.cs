using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

public record CreateUserCommand(
    string Email,
    string Name) : IRequest<User>;
