namespace Axiom.Application.Dtos;

public class SystemDto
{
    public long SystemId { get; init; }
    public string EAI { get; init; } = null!;
    public string Name { get; init; } = null!;
    public Guid OwnerUserId { get; init; }
    public string OwnerName { get; init; } = null!;
}
