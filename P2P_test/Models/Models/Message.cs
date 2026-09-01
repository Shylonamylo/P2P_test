using System;
using System.Xml;

namespace P2P_test.Models.Models;

public class Message
{
    public MessageType Type { get; init; }
    public string Text { get; init; }
    public uint PackageId { get; init; }
    public long SendTime { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public Message(MessageType type, string text, uint packageId)
    {
        Type = type;
        Text = text;
        PackageId = packageId;
    }
}

public enum MessageType : byte
{
    KeepAlive,
    TextMessage,
    Connection,
    Encryption,
    PeerInfo,
    Acknowledge,
}