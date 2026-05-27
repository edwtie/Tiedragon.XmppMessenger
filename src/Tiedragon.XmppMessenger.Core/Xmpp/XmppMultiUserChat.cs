using System.Xml.Linq;

namespace Tiedragon.XmppMessenger.Core.Xmpp;

public static class XmppMultiUserChat
{
    public const string NamespaceName = "http://jabber.org/protocol/muc";

    public const string UserNamespaceName = "http://jabber.org/protocol/muc#user";

    public static XElement CreateJoinPresence(
        XmppAddress room,
        string nickname,
        string? password = null,
        int? historyMaxChars = null,
        XmppAddress? from = null)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentException.ThrowIfNullOrWhiteSpace(nickname);

        var presence = new XElement(XName.Get("presence", XmppXmlNames.ClientNamespace),
            new XAttribute("to", ToOccupantJid(room, nickname).Full),
            new XElement(XName.Get("x", NamespaceName)));

        if (from is not null)
        {
            presence.SetAttributeValue("from", from.Full);
        }

        var x = presence.Element(XName.Get("x", NamespaceName))!;
        if (!string.IsNullOrEmpty(password))
        {
            x.Add(new XElement(XName.Get("password", NamespaceName), password));
        }

        if (historyMaxChars is not null)
        {
            x.Add(new XElement(XName.Get("history", NamespaceName),
                new XAttribute("maxchars", historyMaxChars.Value)));
        }

        return presence;
    }

    public static XElement CreateLeavePresence(XmppAddress room, string nickname)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentException.ThrowIfNullOrWhiteSpace(nickname);

        return new XElement(XName.Get("presence", XmppXmlNames.ClientNamespace),
            new XAttribute("to", ToOccupantJid(room, nickname).Full),
            new XAttribute("type", "unavailable"));
    }

    public static XElement CreateGroupMessage(XmppAddress room, string body, string? id = null)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(body);

        var message = new XElement(XName.Get("message", XmppXmlNames.ClientNamespace),
            new XAttribute("to", room.Bare),
            new XAttribute("type", "groupchat"),
            new XElement(XName.Get("body", XmppXmlNames.ClientNamespace), body));

        if (!string.IsNullOrWhiteSpace(id))
        {
            message.SetAttributeValue("id", id);
        }

        return message;
    }

    public static XElement CreateDirectInvitation(
        XmppAddress invitee,
        XmppAddress room,
        string? reason = null)
    {
        ArgumentNullException.ThrowIfNull(invitee);
        ArgumentNullException.ThrowIfNull(room);

        var x = new XElement(XName.Get("x", "jabber:x:conference"),
            new XAttribute("jid", room.Bare));
        if (!string.IsNullOrWhiteSpace(reason))
        {
            x.SetAttributeValue("reason", reason);
        }

        return new XElement(XName.Get("message", XmppXmlNames.ClientNamespace),
            new XAttribute("to", invitee.Full),
            x);
    }

    public static bool TryParseGroupMessage(XElement element, out XmppGroupChatMessage? message)
    {
        message = null;

        if (element.Name != XName.Get("message", XmppXmlNames.ClientNamespace)
            || !string.Equals((string?)element.Attribute("type"), "groupchat", StringComparison.Ordinal))
        {
            return false;
        }

        XmppAddress.TryParse((string?)element.Attribute("from"), out var from);
        XmppAddress.TryParse((string?)element.Attribute("to"), out var to);
        message = new XmppGroupChatMessage(
            Room: from is null ? null : XmppAddress.Parse(from.Bare),
            Nickname: from?.ResourcePart,
            Body: element.Element(XName.Get("body", XmppXmlNames.ClientNamespace))?.Value ?? string.Empty,
            From: from,
            To: to,
            Id: (string?)element.Attribute("id"));
        return true;
    }

    public static XmppAddress ToOccupantJid(XmppAddress room, string nickname)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentException.ThrowIfNullOrWhiteSpace(nickname);
        return XmppAddress.Parse(room.Bare + "/" + nickname);
    }
}

public sealed record XmppGroupChatMessage(
    XmppAddress? Room,
    string? Nickname,
    string Body,
    XmppAddress? From = null,
    XmppAddress? To = null,
    string? Id = null);
