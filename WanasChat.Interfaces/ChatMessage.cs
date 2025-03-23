namespace WanasChat.Interfaces;

[GenerateSerializer]
[Alias("WanasChat.Interfaces.ChatMessage")]
public class ChatMessage(string username, int loginCount, string message)
{
    [Id(0)]
    public string Username { get; set; } = username;
    [Id(1)]
    public int LoginCount { get; set; } = loginCount;
    [Id(2)]
    public string Message { get; set; } = message;
    [Id(3)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}