using System.Text.Json.Serialization;
using Axiom.Domain.Enums;

namespace Axiom.Domain.ValueObjects;

/// <summary>
/// Represents the status of a knowledge entry as a value object, wrapping the <see cref="Enums.KnowledgeStatus"/> enum.
/// </summary>
[JsonConverter(typeof(KnowledgeStatusValueJsonConverter))]
public readonly record struct KnowledgeStatusValue
{
    /// <summary>Gets the underlying knowledge status enum value.</summary>
    public Enums.KnowledgeStatus Value { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="KnowledgeStatusValue"/>.
    /// </summary>
    /// <param name="value">The knowledge status to wrap.</param>
    public KnowledgeStatusValue(Enums.KnowledgeStatus value)
    {
        Value = value;
    }

    /// <summary>Gets a <see cref="KnowledgeStatusValue"/> representing the Draft status.</summary>
    public static KnowledgeStatusValue Draft => new(Enums.KnowledgeStatus.Draft);
    /// <summary>Gets a <see cref="KnowledgeStatusValue"/> representing the Published status.</summary>
    public static KnowledgeStatusValue Published => new(Enums.KnowledgeStatus.Published);
    /// <summary>Gets a <see cref="KnowledgeStatusValue"/> representing the Archived status.</summary>
    public static KnowledgeStatusValue Archived => new(Enums.KnowledgeStatus.Archived);
    /// <summary>Gets a <see cref="KnowledgeStatusValue"/> representing the Deprecated status.</summary>
    public static KnowledgeStatusValue Deprecated => new(Enums.KnowledgeStatus.Deprecated);

    /// <summary>Returns the string representation of the underlying status value.</summary>
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Custom JSON converter for <see cref="KnowledgeStatusValue"/> that falls back to Draft on parse failure.
/// </summary>
public class KnowledgeStatusValueJsonConverter : System.Text.Json.Serialization.JsonConverter<KnowledgeStatusValue>
{
    /// <summary>Reads and deserializes a <see cref="KnowledgeStatusValue"/> from a JSON string.</summary>
    public override KnowledgeStatusValue Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (Enum.TryParse<Enums.KnowledgeStatus>(value, true, out var result))
            return new KnowledgeStatusValue(result);
        return KnowledgeStatusValue.Draft;
    }

    /// <summary>Writes a <see cref="KnowledgeStatusValue"/> as a JSON string.</summary>
    public override void Write(System.Text.Json.Utf8JsonWriter writer, KnowledgeStatusValue value, System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value.ToString());
    }
}
