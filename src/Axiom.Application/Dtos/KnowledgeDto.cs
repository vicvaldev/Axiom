namespace Axiom.Application.Dtos;

public class KnowledgeDto
{
    public Guid KnowledgeId { get; init; }
    public string Title { get; init; } = null!;
    public string Summary { get; init; } = null!;
    public string SystemName { get; init; } = null!;
    public List<string> Tags { get; init; } = [];
    public string TypeName { get; init; } = null!;
    public string StateName { get; init; } = null!;
    public string CreatedByName { get; init; } = null!;
    public int VersionNumber { get; init; }
    public DateTime UpdatedAt { get; init; }
}
