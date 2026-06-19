namespace Axiom.Application.Dtos;

public class UserDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = null!;
    public string Name { get; init; } = null!;
}
