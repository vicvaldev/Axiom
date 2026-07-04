namespace Axiom.Application.Dtos;

public class JsonKnowledgeEntry
{
    public Guid KnowledgeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public long SystemId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public long KnowledgeTypeId { get; set; }
    public int KnowledgeStateId { get; set; }
    public Guid? IssueId { get; set; }
    public List<string> Tags { get; set; } = [];
    public int VersionNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class JsonIssueEntry
{
    public Guid IssueId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public long SystemId { get; set; }
    public string Problem { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
    public int StateId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? RitmNumber { get; set; }
    public string? IncidentNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class JsonUserEntry
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class JsonSystemEntry
{
    public long SystemId { get; set; }
    public string EAI { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid OwnerUserId { get; set; }
}

public class JsonKnowledgeTypeEntry
{
    public long TypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class JsonKnowledgeStateEntry
{
    public int StateId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class JsonIssueStateEntry
{
    public int StateId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class JsonKnowledgeTagEntry
{
    public long KnowledgeTagId { get; set; }
    public string TagName { get; set; } = string.Empty;
}
