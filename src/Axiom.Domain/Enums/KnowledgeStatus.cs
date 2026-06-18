namespace Axiom.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a knowledge entry.
/// </summary>
public enum KnowledgeStatus
{
    /// <summary>The entry is a draft and not yet ready for general use.</summary>
    Draft,
    /// <summary>The entry has been published and is available for use.</summary>
    Published,
    /// <summary>The entry has been archived and is no longer active but retained for reference.</summary>
    Archived,
    /// <summary>The entry has been deprecated and should no longer be used.</summary>
    Deprecated
}
