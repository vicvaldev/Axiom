using System.Text.Json.Serialization;
using Axiom.Domain.Enums;

namespace Axiom.Domain.ValueObjects;

[JsonConverter(typeof(KnowledgeStatusValueJsonConverter))]
public readonly record struct KnowledgeStatusValue
{
    public Enums.KnowledgeStatus Value { get; }

    public KnowledgeStatusValue(Enums.KnowledgeStatus value)
    {
        Value = value;
    }

    public static KnowledgeStatusValue Draft => new(Enums.KnowledgeStatus.Draft);
    public static KnowledgeStatusValue Published => new(Enums.KnowledgeStatus.Published);
    public static KnowledgeStatusValue Archived => new(Enums.KnowledgeStatus.Archived);
    public static KnowledgeStatusValue Deprecated => new(Enums.KnowledgeStatus.Deprecated);

    public override string ToString() => Value.ToString();
}

public class KnowledgeStatusValueJsonConverter : System.Text.Json.Serialization.JsonConverter<KnowledgeStatusValue>
{
    public override KnowledgeStatusValue Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (Enum.TryParse<Enums.KnowledgeStatus>(value, true, out var result))
            return new KnowledgeStatusValue(result);
        return KnowledgeStatusValue.Draft;
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, KnowledgeStatusValue value, System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value.ToString());
    }
}
