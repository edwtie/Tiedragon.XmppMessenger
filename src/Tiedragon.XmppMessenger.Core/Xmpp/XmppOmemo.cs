using System.Xml.Linq;

namespace Tiedragon.XmppMessenger.Core.Xmpp;

public static class XmppOmemo
{
    public const string NamespaceName = "urn:xmpp:omemo:2";

    public const string DeviceListNode = NamespaceName + ":devices";

    public const string BundleNodePrefix = NamespaceName + ":bundles:";

    public const string PubSubNamespaceName = "http://jabber.org/protocol/pubsub";

    public static string BundleNode(uint deviceId)
    {
        return BundleNodePrefix + deviceId;
    }

    public static XmppIq CreateDeviceListRequest(string id, XmppAddress contact)
    {
        ArgumentNullException.ThrowIfNull(contact);
        return CreatePubSubItemsRequest(id, contact, DeviceListNode);
    }

    public static XmppIq CreateBundleRequest(string id, XmppAddress contact, uint deviceId)
    {
        ArgumentNullException.ThrowIfNull(contact);
        return CreatePubSubItemsRequest(id, contact, BundleNode(deviceId));
    }

    public static XElement CreateEncryptedMessage(
        XmppAddress to,
        uint senderDeviceId,
        IEnumerable<XmppOmemoKeyTransport> keys,
        string payload,
        string? id = null)
    {
        ArgumentNullException.ThrowIfNull(to);
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        var encrypted = new XElement(XName.Get("encrypted", NamespaceName),
            new XElement(XName.Get("header", NamespaceName),
                new XAttribute("sid", senderDeviceId),
                keys.Select(key => new XElement(XName.Get("key", NamespaceName),
                    new XAttribute("rid", key.RecipientDeviceId),
                    key.IsPreKey ? new XAttribute("prekey", "true") : null,
                    key.CipherText))),
            new XElement(XName.Get("payload", NamespaceName), payload));

        var message = new XElement(XName.Get("message", XmppXmlNames.ClientNamespace),
            new XAttribute("to", to.Full),
            new XAttribute("type", "chat"),
            encrypted);
        if (!string.IsNullOrWhiteSpace(id))
        {
            message.SetAttributeValue("id", id);
        }

        return message;
    }

    public static bool TryParseEncryptedMessage(XElement message, out XmppOmemoEncryptedMessage? encryptedMessage)
    {
        encryptedMessage = null;

        if (message.Name != XName.Get("message", XmppXmlNames.ClientNamespace))
        {
            return false;
        }

        var encrypted = message.Element(XName.Get("encrypted", NamespaceName));
        var header = encrypted?.Element(XName.Get("header", NamespaceName));
        if (encrypted is null
            || header is null
            || !uint.TryParse((string?)header.Attribute("sid"), out var senderDeviceId))
        {
            return false;
        }

        var keys = header.Elements(XName.Get("key", NamespaceName))
            .Select(element => uint.TryParse((string?)element.Attribute("rid"), out var rid)
                ? new XmppOmemoKeyTransport(
                    rid,
                    element.Value,
                    string.Equals((string?)element.Attribute("prekey"), "true", StringComparison.OrdinalIgnoreCase))
                : null)
            .Where(key => key is not null)
            .Cast<XmppOmemoKeyTransport>()
            .ToArray();

        XmppAddress.TryParse((string?)message.Attribute("from"), out var from);
        XmppAddress.TryParse((string?)message.Attribute("to"), out var to);
        encryptedMessage = new XmppOmemoEncryptedMessage(
            SenderDeviceId: senderDeviceId,
            Keys: keys,
            Payload: encrypted.Element(XName.Get("payload", NamespaceName))?.Value,
            From: from,
            To: to,
            Id: (string?)message.Attribute("id"));
        return true;
    }

    public static bool TryParseDeviceList(XmppIq iq, out IReadOnlyList<uint> deviceIds)
    {
        deviceIds = Array.Empty<uint>();

        if (iq.Type != XmppIqType.Result || iq.Payload?.Name != XName.Get("pubsub", PubSubNamespaceName))
        {
            return false;
        }

        var ids = iq.Payload
            .Descendants(XName.Get("device", NamespaceName))
            .Select(element => uint.TryParse((string?)element.Attribute("id"), out var id) ? id : (uint?)null)
            .Where(id => id is not null)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        deviceIds = ids;
        return ids.Length > 0;
    }

    private static XmppIq CreatePubSubItemsRequest(string id, XmppAddress contact, string node)
    {
        var pubsub = new XElement(XName.Get("pubsub", PubSubNamespaceName),
            new XElement(XName.Get("items", PubSubNamespaceName),
                new XAttribute("node", node)));
        return new XmppIq(XmppIqType.Get, id, pubsub, To: contact);
    }
}

public sealed record XmppOmemoKeyTransport(
    uint RecipientDeviceId,
    string CipherText,
    bool IsPreKey = false);

public sealed record XmppOmemoEncryptedMessage(
    uint SenderDeviceId,
    IReadOnlyList<XmppOmemoKeyTransport> Keys,
    string? Payload,
    XmppAddress? From = null,
    XmppAddress? To = null,
    string? Id = null);
