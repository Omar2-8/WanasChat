using Microsoft.Extensions.Logging;
using WanasChat.Interfaces;

namespace WanasChat.Grains;
public class RoomGrain(
    [PersistentState("room", "redis")] IPersistentState<RoomState> state,
    ILogger<RoomGrain> logger) : Grain, IRoomGrain
{
    private readonly HashSet<string> _activeUsers = [];

    public Task<List<ChatMessage>> GetRecentMessages(int count)
    {
        var messages = state.State.Messages
            .OrderByDescending(m => m.Timestamp)
            .Take(count)
            .OrderBy(m => m.Timestamp)
            .ToList();

        return Task.FromResult(messages);
    }

    public async Task AddMessage(string username, string message, int loginCount)
    {
        var chatMessage = new ChatMessage(username, loginCount, message);
        state.State.Messages.Add(chatMessage);
         
        if (state.State.Messages.Count > 100)
        {
            state.State.Messages = state.State.Messages
                .OrderByDescending(m => m.Timestamp)
                .Take(100)
                .ToList();
        }

        await state.WriteStateAsync();

        logger.LogInformation("Message added to room {RoomId} by {Username}: {Message}",
            this.GetPrimaryKeyLong(), username, message);
    }

    public Task<int> GetUserCount() => Task.FromResult(_activeUsers.Count);

    public Task UserJoined(string username)
    {
        _activeUsers.Add(username);
        logger.LogInformation("User {Username} joined room {RoomId}", username, this.GetPrimaryKeyLong());
        return Task.CompletedTask;
    }

    public Task UserLeft(string username)
    {
        _activeUsers.Remove(username);
        logger.LogInformation("User {Username} left room {RoomId}", username, this.GetPrimaryKeyLong());
        return Task.CompletedTask;
    }
}
 
public class RoomState
{ 
    public List<ChatMessage> Messages { get; set; } = [];
}