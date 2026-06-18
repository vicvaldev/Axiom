using System.Text.Json.Serialization;

namespace Axiom.Domain.ValueObjects;

/// <summary>
/// Represents a Request Item (RITM) identifier value object.
/// </summary>
[JsonConverter(typeof(RitmIdJsonConverter))]
public readonly record struct RitmId
{
    /// <summary>Gets the underlying string value of the RITM identifier.</summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="RitmId"/>.
    /// </summary>
    /// <param name="value">The RITM identifier string. Must not be null or whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or whitespace.</exception>
    public RitmId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("RITM ID cannot be empty.", nameof(value));

        Value = value;
    }

    /// <summary>Returns the underlying value as a string, or an empty string if null.</summary>
    public override string ToString() => Value ?? string.Empty;
}

/// <summary>
/// Custom JSON converter for <see cref="RitmId"/> that handles null/empty values by falling back to "unknown".
/// </summary>
public class RitmIdJsonConverter : System.Text.Json.Serialization.JsonConverter<RitmId>
{
    /// <summary>Reads and deserializes a <see cref="RitmId"/> from JSON.</summary>
    public override RitmId Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return new RitmId("unknown");
        return new RitmId(value);
    }

    /// <summary>Writes a <see cref="RitmId"/> as a JSON string.</summary>
    public override void Write(System.Text.Json.Utf8JsonWriter writer, RitmId value, System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
