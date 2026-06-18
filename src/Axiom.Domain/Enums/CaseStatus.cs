namespace Axiom.Domain.Enums;

/// <summary>
/// Represents the current state of a case record in its lifecycle.
/// </summary>
public enum CaseStatus
{
    /// <summary>The case has been created but work has not yet begun.</summary>
    Open,
    /// <summary>The case is actively being investigated or worked on.</summary>
    InProgress,
    /// <summary>The case has been resolved and a solution has been implemented.</summary>
    Resolved,
    /// <summary>The case has been closed after resolution and review.</summary>
    Closed
}
