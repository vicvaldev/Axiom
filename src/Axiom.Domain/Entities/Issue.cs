namespace Axiom.Domain.Entities;

public class Issue
{
    public Guid IssueId { get; private set; }
    public string Summary { get; private set; } = null!;
    public string? RitmNumber { get; private set; }
    public string? IncidentNumber { get; private set; }
    public long SystemId { get; private set; }
    public string Problem { get; private set; } = null!;
    public string Analysis { get; private set; } = null!;
    public string Resolution { get; private set; } = null!;
    public int StateId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    public AxiomSystem System { get; private set; } = null!;
    public IssueState State { get; private set; } = null!;
    public User CreatedBy { get; private set; } = null!;
    public ICollection<Knowledge> Knowledges { get; private set; } = new HashSet<Knowledge>();

    private Issue() { }

    public Issue(
        string summary,
        long systemId,
        string problem,
        int stateId,
        Guid createdByUserId,
        string? analysis = null,
        string? resolution = null,
        string? ritmNumber = null,
        string? incidentNumber = null)
    {
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Summary cannot be empty.", nameof(summary));
        if (string.IsNullOrWhiteSpace(problem))
            throw new ArgumentException("Problem cannot be empty.", nameof(problem));

        IssueId = Guid.NewGuid();
        Summary = summary;
        SystemId = systemId;
        Problem = problem;
        Analysis = analysis ?? string.Empty;
        Resolution = resolution ?? string.Empty;
        StateId = stateId;
        CreatedByUserId = createdByUserId;
        RitmNumber = ritmNumber;
        IncidentNumber = incidentNumber;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Resolve(int stateId, string resolution)
    {
        StateId = stateId;
        Resolution = resolution ?? string.Empty;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateState(int stateId)
    {
        StateId = stateId;
        UpdatedAt = DateTime.UtcNow;
    }
}
