namespace WanasChat.Interfaces;

[Alias("IRoomGrain")]
public interface IRoomGrain : IGrainWithIntegerKey
{
    [Alias("GetRecentMessages")]
    Task<List<ChatMessage>> GetRecentMessages(int count);
    [Alias("AddMessage")]
    Task AddMessage(string username, string message, int loginCount);
    [Alias("GetUserCount")]
    Task<int> GetUserCount();
    [Alias("UserJoined")]
    Task UserJoined(string username);
    [Alias("UserLeft")]
    Task UserLeft(string username);
}