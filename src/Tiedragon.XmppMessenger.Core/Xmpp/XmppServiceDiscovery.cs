using System.Xml.Linq;

namespace Tiedragon.XmppMessenger.Core.Xmpp;

public static class XmppServiceDiscovery
{
    public const string InfoNamespace = "http://jabber.org/protocol/disco#info";

    public const string DataFormNamespace = "jabber:x:data";

    public static bool SupportsRealTimeText(XmppServiceDiscoveryInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        return info.Supports(Tiedragon.XmppMessenger.Core.Rtt.RttPacket.NamespaceName);
    }

    public static XmppIq CreateInfoRequest(string id, XmppAddress? to = null, string? node = null)
    {
        var query = new XElement(XName.Get("query", InfoNamespace));
        if (!string.IsNullOrWhiteSpace(node))
        {
            query.SetAttributeValue("node", node);
        }

        return new XmppIq(XmppIqType.Get, id, query, To: to);
    }

    public static bool TryParseInfoResult(XmppIq iq, out XmppServiceDiscoveryInfo? info)
    {
        info = null;

        if (iq.Type != XmppIqType.Result || iq.Payload?.Name != XName.Get("query", InfoNamespace))
        {
            return false;
        }

        var identities = iq.Payload.Elements(XName.Get("identity", InfoNamespace))
            .Select(element => new XmppServiceIdentity(
                Category: (string?)element.Attribute("category") ?? string.Empty,
                Type: (string?)element.Attribute("type") ?? string.Empty,
                Name: (string?)element.Attribute("name"),
                Language: (string?)element.Attribute(XNamespace.Xml + "lang")))
            .Where(identity => identity.Category.Length > 0 || identity.Type.Length > 0)
            .ToArray();

        var features = iq.Payload.Elements(XName.Get("feature", InfoNamespace))
            .Select(element => (string?)element.Attribute("var"))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var dataForms = iq.Payload.Elements(XName.Get("x", DataFormNamespace))
            .Select(ParseDataForm)
            .Where(form => form.Fields.Count > 0)
            .ToArray();

        info = new XmppServiceDiscoveryInfo(
            Node: (string?)iq.Payload.Attribute("node"),
            Identities: identities,
            Features: features,
            DataForms: dataForms);
        return true;
    }

    private static XmppDataForm ParseDataForm(XElement element)
    {
        var fields = element.Elements(XName.Get("field", DataFormNamespace))
            .Select(field => (
                Name: (string?)field.Attribute("var"),
                Values: field.Elements(XName.Get("value", DataFormNamespace))
                    .Select(value => value.Value)
                    .ToArray()))
            .Where(field => !string.IsNullOrWhiteSpace(field.Name))
            .ToDictionary(
                field => field.Name!,
                field => (IReadOnlyList<string>)field.Values,
                StringComparer.Ordinal);

        return new XmppDataForm((string?)element.Attribute("type"), fields);
    }
}

public sealed record XmppServiceDiscoveryInfo
{
    public XmppServiceDiscoveryInfo(
        string? Node,
        IReadOnlyList<XmppServiceIdentity> Identities,
        IReadOnlyList<string> Features)
        : this(Node, Identities, Features, [])
    {
    }

    public XmppServiceDiscoveryInfo(
        string? Node,
        IReadOnlyList<XmppServiceIdentity> Identities,
        IReadOnlyList<string> Features,
        IReadOnlyList<XmppDataForm> DataForms)
    {
        this.Node = Node;
        this.Identities = Identities;
        this.Features = Features;
        this.DataForms = DataForms;
    }

    public string? Node { get; init; }

    public IReadOnlyList<XmppServiceIdentity> Identities { get; init; }

    public IReadOnlyList<string> Features { get; init; }

    public IReadOnlyList<XmppDataForm> DataForms { get; init; }

    public bool Supports(string feature)
    {
        return Features.Any(value => string.Equals(value, feature, StringComparison.Ordinal));
    }
}

public sealed record XmppServiceIdentity(
    string Category,
    string Type,
    string? Name = null,
    string? Language = null);

public sealed record XmppDataForm(
    string? Type,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Fields)
{
    public string? FormType => GetFirstValue("FORM_TYPE");

    public string? GetFirstValue(string fieldName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
        return Fields.TryGetValue(fieldName, out var values) ? values.FirstOrDefault() : null;
    }
}
