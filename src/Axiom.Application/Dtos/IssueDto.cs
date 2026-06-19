namespace Axiom.Application.Dtos;

public class IssueDto
{
    public Guid IssueId { get; init; }
    public string Summary { get; init; } = null!;
    public string SystemName { get; init; } = null!;
    public string StateName { get; init; } = null!;
    public string? RitmNumber { get; init; }
    public string? IncidentNumber { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
}
