namespace Axiom.Infrastructure.Data;

public class JsonDataOptions
{
    public const string SectionName = "JsonData";

    public string KnowledgeFilePath { get; set; } = "data/knowledge.json";
    public string CasesFilePath { get; set; } = "data/cases.json";
}
