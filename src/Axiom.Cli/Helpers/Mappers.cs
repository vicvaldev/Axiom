using Axiom.Domain.Entities;

namespace Axiom.Cli.Helpers;

internal static class Mappers
{
    public static object ToKnowledgeCreateResult(Knowledge entry)
    {
        return new
        {
            entry.KnowledgeId,
            entry.Title,
            entry.Summary,
            entry.Content,
            entry.SystemId,
            entry.CreatedByUserId,
            entry.KnowledgeTypeId,
            entry.KnowledgeStateId,
            entry.IssueId,
            entry.VersionNumber,
            entry.CreatedAt,
            entry.UpdatedAt
        };
    }

    public static object ToKnowledgeDetails(Knowledge entry)
    {
        return new
        {
            entry.KnowledgeId,
            entry.Title,
            entry.Summary,
            entry.Content,
            entry.SystemId,
            SystemName = entry.System?.Name,
            entry.CreatedByUserId,
            CreatedByName = entry.CreatedBy?.Name,
            entry.KnowledgeTypeId,
            TypeName = entry.Type?.Name,
            entry.KnowledgeStateId,
            StateName = entry.State?.Name,
            entry.IssueId,
            Tags = entry.KnowledgeKnowledgeTags
                .Select(t => t.Tag?.TagName)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList(),
            entry.VersionNumber,
            entry.CreatedAt,
            entry.UpdatedAt
        };
    }

    public static object ToIssueCreateResult(Issue issue)
    {
        return new
        {
            issue.IssueId,
            issue.Summary,
            issue.SystemId,
            issue.StateId,
            issue.CreatedByUserId,
            issue.RitmNumber,
            issue.IncidentNumber,
            issue.CreatedAt,
            issue.UpdatedAt,
            issue.ResolvedAt
        };
    }

    public static object ToIssueDetails(Issue issue)
    {
        return new
        {
            issue.IssueId,
            issue.Summary,
            issue.SystemId,
            SystemName = issue.System?.Name,
            issue.StateId,
            StateName = issue.State?.Name,
            issue.CreatedByUserId,
            CreatedByName = issue.CreatedBy?.Name,
            issue.RitmNumber,
            issue.IncidentNumber,
            issue.Problem,
            issue.Analysis,
            issue.Resolution,
            issue.CreatedAt,
            issue.UpdatedAt,
            issue.ResolvedAt
        };
    }
}
