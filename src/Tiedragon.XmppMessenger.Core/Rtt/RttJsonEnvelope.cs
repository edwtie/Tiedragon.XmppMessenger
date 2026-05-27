using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tiedragon.XmppMessenger.Core.Rtt;

public sealed record RttJsonEnvelope(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("xml")] string Xml,
    [property: JsonPropertyName("sender")] string? Sender = null)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static RttJsonEnvelope FromPacket(RttPacket packet, string text, string? sender = null)
    {
        ArgumentNullException.ThrowIfNull(packet);
        return new RttJsonEnvelope("rtt", text, packet.ToXml(), NormalizeSender(sender));
    }

    public static RttJsonEnvelope FromTextMessage(string text, string? sender = null)
    {
        return new RttJsonEnvelope("message", text ?? string.Empty, string.Empty, NormalizeSender(sender));
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

    private static string? NormalizeSender(string? sender)
    {
        return string.IsNullOrWhiteSpace(sender) ? null : sender.Trim();
    }
}
