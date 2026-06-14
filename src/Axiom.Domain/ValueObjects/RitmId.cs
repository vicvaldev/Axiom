using System.Text.Json.Serialization;

namespace Axiom.Domain.ValueObjects;

[JsonConverter(typeof(RitmIdJsonConverter))]
public readonly record struct RitmId
{
    public string Value { get; }

    public RitmId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("RITM ID cannot be empty.", nameof(value));

        Value = value;
    }

    public override string ToString() => Value ?? string.Empty;
}

public class RitmIdJsonConverter : System.Text.Json.Serialization.JsonConverter<RitmId>
{
    public override RitmId Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return new RitmId("unknown");
        return new RitmId(value);
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, RitmId value, System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
