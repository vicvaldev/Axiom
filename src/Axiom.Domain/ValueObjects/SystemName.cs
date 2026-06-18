using System.Text.Json.Serialization;

namespace Axiom.Domain.ValueObjects;

/// <summary>
/// Represents a system or application name value object.
/// </summary>
[JsonConverter(typeof(SystemNameJsonConverter))]
public readonly record struct SystemName
{
    /// <summary>Gets the underlying string value of the system name.</summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="SystemName"/>.
    /// </summary>
    /// <param name="value">The system name string. Must not be null or whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or whitespace.</exception>
    public SystemName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("System name cannot be empty.", nameof(value));

        Value = value;
    }

    /// <summary>Returns the underlying value as a string, or an empty string if null.</summary>
    public override string ToString() => Value ?? string.Empty;
}

/// <summary>
/// Custom JSON converter for <see cref="SystemName"/> that handles null/empty values by falling back to "unknown".
/// </summary>
public class SystemNameJsonConverter : System.Text.Json.Serialization.JsonConverter<SystemName>
{
    /// <summary>Reads and deserializes a <see cref="SystemName"/> from JSON.</summary>
    public override SystemName Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return new SystemName("unknown");
        return new SystemName(value);
    }

    /// <summary>Writes a <see cref="SystemName"/> as a JSON string.</summary>
    public override void Write(System.Text.Json.Utf8JsonWriter writer, SystemName value, System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
