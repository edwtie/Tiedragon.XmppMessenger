using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tiedragon.XmppMessenger.Core.Rtt;

public sealed record RttJsonEnvelope(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("xml")] string Xml,
    [property: JsonPropertyName("from")] string? From = null,
    [property: JsonPropertyName("to")] string? To = null)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static RttJsonEnvelope FromPacket(RttPacket packet, string text, string? from = null, string? to = null)
    {
        ArgumentNullException.ThrowIfNull(packet);
        return new RttJsonEnvelope("rtt", text, packet.ToXml(), NormalizeAddress(from), NormalizeAddress(to));
    }

    public static RttJsonEnvelope FromTextMessage(string text, string? from = null, string? to = null)
    {
        return new RttJsonEnvelope("message", text ?? string.Empty, string.Empty, NormalizeAddress(from), NormalizeAddress(to));
    }

    public static bool TryParse(string json, out RttJsonEnvelope? envelope)
    {
        envelope = null;

        try
        {
            var parsed = JsonSerializer.Deserialize<RttJsonEnvelope>(json, SerializerOptions);
            if (parsed is null)
            {
                return false;
            }

            if (parsed.Type == "rtt" && string.IsNullOrWhiteSpace(parsed.Xml))
            {
                return false;
            }

            if (parsed.Type != "rtt" && parsed.Type != "message")
            {
                return false;
            }

            envelope = parsed;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, SerializerOptions);
    }

    private static string? NormalizeAddress(string? address)
    {
        return string.IsNullOrWhiteSpace(address) ? null : address.Trim();
    }
}
