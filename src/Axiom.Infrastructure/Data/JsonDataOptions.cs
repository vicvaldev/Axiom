namespace Axiom.Infrastructure.Data;

/// <summary>
/// Configuration options for JSON file-based persistence paths.
/// Bound to the <c>"JsonData"</c> configuration section.
/// </summary>
public class JsonDataOptions
{
    /// <summary>The default configuration section name used to bind these options.</summary>
    public const string SectionName = "JsonData";

    /// <summary>Gets or sets the file path for the knowledge entries JSON store. Defaults to <c>data/knowledge.json</c>.</summary>
    public string KnowledgeFilePath { get; set; } = "data/knowledge.json";
    /// <summary>Gets or sets the file path for the case records JSON store. Defaults to <c>data/cases.json</c>.</summary>
    public string CasesFilePath { get; set; } = "data/cases.json";
}
