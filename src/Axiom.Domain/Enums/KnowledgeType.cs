namespace Axiom.Domain.Enums;

/// <summary>
/// Categorizes a knowledge entry by its content type.
/// </summary>
public enum KnowledgeType
{
    /// <summary>Formal documentation articles and guides.</summary>
    Documentation,
    /// <summary>Operational runbooks and standard procedures.</summary>
    Runbook,
    /// <summary>Troubleshooting steps and issue resolutions.</summary>
    Troubleshooting,
    /// <summary>Reference materials and quick-lookup content.</summary>
    Reference,
    /// <summary>Educational tutorials and step-by-step instructions.</summary>
    Tutorial,
    /// <summary>Any other type of knowledge content not covered by the other categories.</summary>
    Other
}
