using System.Xml.Linq;

namespace Tiedragon.XmppMessenger.Core.Xmpp;

public static class XmppJingle
{
    public const string NamespaceName = "urn:xmpp:jingle:1";

    public const string RtpNamespaceName = "urn:xmpp:jingle:apps:rtp:1";

    public const string IceUdpNamespaceName = "urn:xmpp:jingle:transports:ice-udp:1";

    public static XmppIq CreateSessionInitiate(
        string id,
        XmppAddress to,
        string sid,
        string creator,
        IEnumerable<XmppJingleContent> contents)
    {
        return CreateJingleIq(id, to, "session-initiate", sid, creator, contents);
    }

    public static XmppIq CreateSessionAccept(
        string id,
        XmppAddress to,
        string sid,
        string creator,
        IEnumerable<XmppJingleContent> contents)
    {
        return CreateJingleIq(id, to, "session-accept", sid, creator, contents);
    }

    public static XmppIq CreateSessionTerminate(
        string id,
        XmppAddress to,
        string sid,
        string reason = "success",
        string? text = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(to);
        ArgumentException.ThrowIfNullOrWhiteSpace(sid);

        var reasonElement = new XElement(XName.Get("reason", NamespaceName),
            new XElement(XName.Get(reason, NamespaceName)));
        if (!string.IsNullOrWhiteSpace(text))
        {
            reasonElement.Add(new XElement(XName.Get("text", NamespaceName), text));
        }

        var jingle = new XElement(XName.Get("jingle", NamespaceName),
            new XAttribute("action", "session-terminate"),
            new XAttribute("sid", sid),
            reasonElement);
        return new XmppIq(XmppIqType.Set, id, jingle, To: to);
    }

    public static XmppJingleContent CreateRtpContent(
        string name,
        string media,
        IEnumerable<XmppJinglePayloadType> payloadTypes,
        string creator = "initiator",
        string senders = "both")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(media);
        ArgumentNullException.ThrowIfNull(payloadTypes);

        var description = new XElement(XName.Get("description", RtpNamespaceName),
            new XAttribute("media", media),
            payloadTypes.Select(payload => new XElement(XName.Get("payload-type", RtpNamespaceName),
                new XAttribute("id", payload.Id),
                new XAttribute("name", payload.Name),
                payload.ClockRate is null ? null : new XAttribute("clockrate", payload.ClockRate.Value),
                payload.Channels is null ? null : new XAttribute("channels", payload.Channels.Value))));

        return new XmppJingleContent(name, creator, senders, description, new XElement(XName.Get("transport", IceUdpNamespaceName)));
    }

    public static bool TryParse(XmppIq iq, out XmppJingleSession? session)
    {
        session = null;

        if (iq.Payload?.Name != XName.Get("jingle", NamespaceName))
        {
            return false;
        }

        var sid = (string?)iq.Payload.Attribute("sid");
        var action = (string?)iq.Payload.Attribute("action");
        if (string.IsNullOrWhiteSpace(sid) || string.IsNullOrWhiteSpace(action))
        {
            return false;
        }

        var contents = iq.Payload.Elements(XName.Get("content", NamespaceName))
            .Select(element => new XmppJingleContent(
                Name: (string?)element.Attribute("name") ?? string.Empty,
                Creator: (string?)element.Attribute("creator") ?? string.Empty,
                Senders: (string?)element.Attribute("senders") ?? string.Empty,
                Description: element.Elements().FirstOrDefault(child => child.Name.LocalName == "description"),
                Transport: element.Elements().FirstOrDefault(child => child.Name.LocalName == "transport")))
            .ToArray();

        session = new XmppJingleSession(
            Sid: sid,
            Action: action,
            Initiator: (string?)iq.Payload.Attribute("initiator"),
            Responder: (string?)iq.Payload.Attribute("responder"),
            Contents: contents);
        return true;
    }

    private static XmppIq CreateJingleIq(
        string id,
        XmppAddress to,
        string action,
        string sid,
        string creator,
        IEnumerable<XmppJingleContent> contents)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(to);
        ArgumentException.ThrowIfNullOrWhiteSpace(sid);
        ArgumentException.ThrowIfNullOrWhiteSpace(creator);
        ArgumentNullException.ThrowIfNull(contents);

        var jingle = new XElement(XName.Get("jingle", NamespaceName),
            new XAttribute("action", action),
            new XAttribute("sid", sid),
            contents.Select(content => content.ToXml()));
        return new XmppIq(XmppIqType.Set, id, jingle, To: to);
    }
}

public sealed record XmppJingleSession(
    string Sid,
    string Action,
    string? Initiator,
    string? Responder,
    IReadOnlyList<XmppJingleContent> Contents);

public sealed record XmppJingleContent(
    string Name,
    string Creator,
    string Senders,
    XElement? Description,
    XElement? Transport)
{
    public XElement ToXml()
    {
        var content = new XElement(XName.Get("content", XmppJingle.NamespaceName),
            new XAttribute("creator", Creator),
            new XAttribute("name", Name));
        if (!string.IsNullOrWhiteSpace(Senders))
        {
            content.SetAttributeValue("senders", Senders);
        }

        if (Description is not null)
        {
            content.Add(new XElement(Description));
        }

        if (Transport is not null)
        {
            content.Add(new XElement(Transport));
        }

        return content;
    }
}

public sealed record XmppJinglePayloadType(
    int Id,
    string Name,
    int? ClockRate = null,
    int? Channels = null);
