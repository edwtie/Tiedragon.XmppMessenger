using System.Buffers.Binary;
using System.Text;

namespace Tiedragon.XmppMessenger.Core.Xmpp;

public static class T140Codec
{
    public const char Backspace = '\b';
    public const char Delete = '\u007f';
    public const char ReplacementCharacter = '\ufffd';

    public static byte[] EncodeBlock(string text)
    {
        return Encoding.UTF8.GetBytes(text ?? string.Empty);
    }

    public static string DecodeBlock(ReadOnlySpan<byte> payload)
    {
        return Encoding.UTF8.GetString(payload);
    }

    public static string ApplyBlock(string currentText, ReadOnlySpan<byte> payload)
    {
        return ApplyText(currentText, DecodeBlock(payload));
    }

    public static string ApplyText(string currentText, string text)
    {
        var result = new StringBuilder(currentText ?? string.Empty);
        var previousWasCarriageReturn = false;

        foreach (var rune in (text ?? string.Empty).EnumerateRunes())
        {
            if (rune.Value is Backspace or Delete)
            {
                RemoveLastRune(result);
                previousWasCarriageReturn = false;
                continue;
            }

            if (rune.Value == '\r')
            {
                result.Append('\n');
                previousWasCarriageReturn = true;
                continue;
            }

            if (rune.Value == '\n')
            {
                if (!previousWasCarriageReturn)
                {
                    result.Append('\n');
                }

                previousWasCarriageReturn = false;
                continue;
            }

            previousWasCarriageReturn = false;
            if (IsIgnoredControl(rune))
            {
                continue;
            }

            result.Append(rune);
        }

        return result.ToString();
    }

    public static string CreateBackspaces(int count)
    {
        if (count <= 0)
        {
            return string.Empty;
        }

        return new string(Backspace, count);
    }

    public static string CreateMissingTextMarker()
    {
        return ReplacementCharacter.ToString();
    }

    private static bool IsIgnoredControl(Rune rune)
    {
        return rune.Value != '\t'
            && (rune.Value < 0x20 || (rune.Value >= 0x80 && rune.Value <= 0x9f));
    }

    private static void RemoveLastRune(StringBuilder value)
    {
        if (value.Length == 0)
        {
            return;
        }

        var last = value[value.Length - 1];
        if (char.IsLowSurrogate(last) && value.Length > 1 && char.IsHighSurrogate(value[value.Length - 2]))
        {
            value.Remove(value.Length - 2, 2);
            return;
        }

        value.Remove(value.Length - 1, 1);
    }
}

public sealed class RtpPacket
{
    public const int FixedHeaderLength = 12;

    public RtpPacket(byte payloadType, ushort sequenceNumber, uint timestamp, uint ssrc, ReadOnlySpan<byte> payload, bool marker = false)
    {
        if (payloadType > 127)
        {
            throw new ArgumentOutOfRangeException(nameof(payloadType), "RTP payload type must fit in 7 bits.");
        }

        PayloadType = payloadType;
        SequenceNumber = sequenceNumber;
        Timestamp = timestamp;
        Ssrc = ssrc;
        Marker = marker;
        Payload = payload.ToArray();
    }

    public byte PayloadType { get; }

    public ushort SequenceNumber { get; }

    public uint Timestamp { get; }

    public uint Ssrc { get; }

    public bool Marker { get; }

    public byte[] Payload { get; }

    public byte[] ToBytes()
    {
        var packet = new byte[FixedHeaderLength + Payload.Length];
        packet[0] = 0x80;
        packet[1] = (byte)((Marker ? 0x80 : 0) | PayloadType);
        BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(2), SequenceNumber);
        BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(4), Timestamp);
        BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(8), Ssrc);
        Payload.CopyTo(packet.AsSpan(FixedHeaderLength));
        return packet;
    }

    public static RtpPacket Parse(ReadOnlySpan<byte> packet)
    {
        if (packet.Length < FixedHeaderLength)
        {
            throw new FormatException("RTP packet is shorter than the fixed header.");
        }

        var version = packet[0] >> 6;
        if (version != 2)
        {
            throw new FormatException("Only RTP version 2 is supported.");
        }

        var hasPadding = (packet[0] & 0x20) != 0;
        var hasExtension = (packet[0] & 0x10) != 0;
        var csrcCount = packet[0] & 0x0f;
        var headerLength = FixedHeaderLength + (csrcCount * 4);
        if (packet.Length < headerLength)
        {
            throw new FormatException("RTP packet is shorter than its CSRC header.");
        }

        if (hasExtension)
        {
            if (packet.Length < headerLength + 4)
            {
                throw new FormatException("RTP packet extension header is incomplete.");
            }

            var extensionWords = BinaryPrimitives.ReadUInt16BigEndian(packet.Slice(headerLength + 2, 2));
            headerLength += 4 + (extensionWords * 4);
            if (packet.Length < headerLength)
            {
                throw new FormatException("RTP packet extension body is incomplete.");
            }
        }

        var payloadLength = packet.Length - headerLength;
        if (hasPadding)
        {
            var paddingLength = packet[packet.Length - 1];
            if (paddingLength == 0 || paddingLength > payloadLength)
            {
                throw new FormatException("RTP padding length is invalid.");
            }

            payloadLength -= paddingLength;
        }

        return new RtpPacket(
            (byte)(packet[1] & 0x7f),
            BinaryPrimitives.ReadUInt16BigEndian(packet.Slice(2, 2)),
            BinaryPrimitives.ReadUInt32BigEndian(packet.Slice(4, 4)),
            BinaryPrimitives.ReadUInt32BigEndian(packet.Slice(8, 4)),
            packet.Slice(headerLength, payloadLength),
            (packet[1] & 0x80) != 0);
    }
}

public sealed class RtpT140Packetizer
{
    public const int ClockRate = 1000;
    public const byte DefaultTextPayloadType = 98;
    public const byte DefaultRedPayloadType = 100;

    private readonly uint _ssrc;
    private readonly byte _payloadType;
    private ushort _sequenceNumber;
    private bool _firstPacket = true;

    public RtpT140Packetizer(uint ssrc, ushort initialSequenceNumber = 0, byte payloadType = DefaultTextPayloadType)
    {
        _ssrc = ssrc;
        _sequenceNumber = initialSequenceNumber;
        _payloadType = payloadType;
    }

    public RtpPacket CreatePacket(string text, uint timestampMilliseconds)
    {
        var packet = CreatePacket(
            text,
            _sequenceNumber,
            timestampMilliseconds,
            _ssrc,
            _payloadType,
            marker: _firstPacket);

        _sequenceNumber++;
        _firstPacket = false;
        return packet;
    }

    public static RtpPacket CreatePacket(
        string text,
        ushort sequenceNumber,
        uint timestampMilliseconds,
        uint ssrc,
        byte payloadType = DefaultTextPayloadType,
        bool marker = false)
    {
        return new RtpPacket(
            payloadType,
            sequenceNumber,
            timestampMilliseconds,
            ssrc,
            T140Codec.EncodeBlock(text),
            marker);
    }

    public static string ReadText(RtpPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        return T140Codec.DecodeBlock(packet.Payload);
    }
}

public sealed class RtpT140RedundantBlock
{
    public RtpT140RedundantBlock(byte payloadType, ushort timestampOffset, ReadOnlySpan<byte> payload)
    {
        if (payloadType > 127)
        {
            throw new ArgumentOutOfRangeException(nameof(payloadType), "RTP payload type must fit in 7 bits.");
        }

        if (payload.Length > 1023)
        {
            throw new ArgumentOutOfRangeException(nameof(payload), "RFC 2198 block length must fit in 10 bits.");
        }

        PayloadType = payloadType;
        TimestampOffset = timestampOffset;
        Payload = payload.ToArray();
    }

    public byte PayloadType { get; }

    public ushort TimestampOffset { get; }

    public byte[] Payload { get; }
}

public sealed class RtpT140RedundantPayload
{
    public RtpT140RedundantPayload(byte primaryPayloadType, ReadOnlySpan<byte> primaryPayload, IReadOnlyList<RtpT140RedundantBlock> redundantBlocks)
    {
        if (primaryPayloadType > 127)
        {
            throw new ArgumentOutOfRangeException(nameof(primaryPayloadType), "RTP payload type must fit in 7 bits.");
        }

        PrimaryPayloadType = primaryPayloadType;
        PrimaryPayload = primaryPayload.ToArray();
        RedundantBlocks = redundantBlocks.ToArray();
    }

    public byte PrimaryPayloadType { get; }

    public byte[] PrimaryPayload { get; }

    public IReadOnlyList<RtpT140RedundantBlock> RedundantBlocks { get; }

    public static byte[] Create(byte primaryPayloadType, ReadOnlySpan<byte> primaryPayload, params RtpT140RedundantBlock[] redundantBlocks)
    {
        var headerLength = (redundantBlocks.Length * 4) + 1;
        var payloadLength = primaryPayload.Length + redundantBlocks.Sum(block => block.Payload.Length);
        var result = new byte[headerLength + payloadLength];
        var offset = 0;

        foreach (var block in redundantBlocks)
        {
            result[offset++] = (byte)(0x80 | block.PayloadType);
            var combined = ((block.TimestampOffset & 0x3fff) << 10) | (block.Payload.Length & 0x03ff);
            result[offset++] = (byte)((combined >> 16) & 0xff);
            result[offset++] = (byte)((combined >> 8) & 0xff);
            result[offset++] = (byte)(combined & 0xff);
        }

        result[offset++] = (byte)(primaryPayloadType & 0x7f);
        foreach (var block in redundantBlocks)
        {
            block.Payload.CopyTo(result.AsSpan(offset));
            offset += block.Payload.Length;
        }

        primaryPayload.CopyTo(result.AsSpan(offset));
        return result;
    }

    public static RtpT140RedundantPayload Parse(ReadOnlySpan<byte> payload)
    {
        if (payload.Length == 0)
        {
            throw new FormatException("RFC 2198 redundant payload is empty.");
        }

        var offset = 0;
        var headers = new List<(byte PayloadType, ushort TimestampOffset, int Length)>();
        byte primaryPayloadType;
        while (true)
        {
            var header = payload[offset++];
            var hasMoreHeaders = (header & 0x80) != 0;
            var payloadType = (byte)(header & 0x7f);
            if (!hasMoreHeaders)
            {
                primaryPayloadType = payloadType;
                break;
            }

            if (payload.Length < offset + 3)
            {
                throw new FormatException("RFC 2198 redundant block header is incomplete.");
            }

            var combined = (payload[offset] << 16) | (payload[offset + 1] << 8) | payload[offset + 2];
            offset += 3;
            headers.Add((payloadType, (ushort)((combined >> 10) & 0x3fff), combined & 0x03ff));
        }

        var blocks = new List<RtpT140RedundantBlock>();
        foreach (var header in headers)
        {
            if (payload.Length < offset + header.Length)
            {
                throw new FormatException("RFC 2198 redundant block body is incomplete.");
            }

            blocks.Add(new RtpT140RedundantBlock(
                header.PayloadType,
                header.TimestampOffset,
                payload.Slice(offset, header.Length)));
            offset += header.Length;
        }

        return new RtpT140RedundantPayload(primaryPayloadType, payload[offset..], blocks);
    }
}
