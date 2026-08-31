using System;
using System.Xml;

namespace P2P_test.Models.Models;

public class Message
{
    public MessageType Type { get; set; }
    public string Text { get; set; }
    public uint PackageID { get; }

    public Message(MessageType type, string text, uint packageID)
    {
        Type = type;
        Text = text;
        PackageID = packageID;
    }
}

public enum MessageType
{
    KeepAlive,
    TextMessage,
    Connection,
    Encryption,
    PeerInfo
}