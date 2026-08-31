namespace P2P_test.Models.Models;

public class ChatMessage
{
    public string Text { get; set; }
    public bool IsMine { get; set; }
    public ChatMessage(string text, bool isMine)
    {
        Text = text;
        IsMine = isMine;
    }
}