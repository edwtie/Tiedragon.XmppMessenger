using System.Xml.Linq;

namespace Tiedragon.XmppMessenger.Core.Xmpp;

public static class XmppHttpFileUpload
{
    public const string NamespaceName = "urn:xmpp:http:upload:0";

    public const string PurposeNamespaceName = "urn:xmpp:http:upload:purpose:0";

    private static readonly HashSet<string> AllowedHeaderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "Expires"
    };

    public static XmppIq CreateSlotRequest(
        string id,
        XmppAddress uploadService,
        string fileName,
        long size,
        string? contentType = null,
        XmppHttpUploadPurpose purpose = XmppHttpUploadPurpose.Default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(uploadService);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (size < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "File size cannot be negative.");
        }

        var request = new XElement(XName.Get("request", NamespaceName),
            new XAttribute("filename", fileName),
            new XAttribute("size", size));

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            request.SetAttributeValue("content-type", contentType);
        }

        if (purpose is XmppHttpUploadPurpose.Message or XmppHttpUploadPurpose.Profile)
        {
            request.Add(new XElement(XName.Get(purpose == XmppHttpUploadPurpose.Message ? "message" : "profile", PurposeNamespaceName)));
        }

        return new XmppIq(XmppIqType.Get, id, request, To: uploadService);
    }

    public static bool TryParseSlotResult(XmppIq iq, out XmppHttpUploadSlot? slot)
    {
        slot = null;

        if (iq.Type != XmppIqType.Result || iq.Payload?.Name != XName.Get("slot", NamespaceName))
        {
            return false;
        }

        var put = iq.Payload.Element(XName.Get("put", NamespaceName));
        var get = iq.Payload.Element(XName.Get("get", NamespaceName));
        var putUrl = (string?)put?.Attribute("url");
        var getUrl = (string?)get?.Attribute("url");
        if (!Uri.TryCreate(putUrl, UriKind.Absolute, out var putUri)
            || !Uri.TryCreate(getUrl, UriKind.Absolute, out var getUri))
        {
            return false;
        }

        var headers = put?.Elements(XName.Get("header", NamespaceName))
            .Select(element => new XmppHttpUploadHeader(
                SanitizeHeader((string?)element.Attribute("name") ?? string.Empty),
                SanitizeHeader(element.Value)))
            .Where(header => AllowedHeaderNames.Contains(header.Name))
            .ToArray() ?? [];

        slot = new XmppHttpUploadSlot(putUri, getUri, headers);
        return true;
    }

    private static string SanitizeHeader(string value)
    {
        return value.Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal);
    }
}

public enum XmppHttpUploadPurpose
{
    Default,
    Message,
    Profile
}

public sealed record XmppHttpUploadSlot(
    Uri PutUrl,
    Uri GetUrl,
    IReadOnlyList<XmppHttpUploadHeader> Headers);

public sealed record XmppHttpUploadHeader(string Name, string Value);
