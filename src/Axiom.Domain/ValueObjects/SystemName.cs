using System.Text.Json.Serialization;

namespace Axiom.Domain.ValueObjects;

[JsonConverter(typeof(SystemNameJsonConverter))]
public readonly record struct SystemName
{
    public string Value { get; }

    public SystemName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("System name cannot be empty.", nameof(value));

        Value = value;
    }

    public override string ToString() => Value ?? string.Empty;
}

public class SystemNameJsonConverter : System.Text.Json.Serialization.JsonConverter<SystemName>
{
    public override SystemName Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return new SystemName("unknown");
        return new SystemName(value);
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, SystemName value, System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
