using Microsoft.Extensions.Logging;
using WanasChat.Interfaces;

namespace WanasChat.Grains;

public class UserGrain(
    [PersistentState("user", "redis")] IPersistentState<UserState> state,
    ILogger<UserGrain> logger) : Grain, IUserGrain
{
    public Task<int> GetLoginCount() => Task.FromResult(state.State.LoginCount);

    public async Task IncrementLoginCount()
    {
        state.State.LoginCount++;
        await state.WriteStateAsync();
        logger.LogInformation("User {UserId} login count incremented to {Count}", this.GetPrimaryKeyString(), state.State.LoginCount);
    }

    public Task<int> GetCurrentRoom() => Task.FromResult(state.State.CurrentRoom);

    public async Task SetCurrentRoom(int roomNumber)
    {
        if (roomNumber <= 0)
            throw new ArgumentException("Room number must be greater than zero", nameof(roomNumber));

        var oldRoom = state.State.CurrentRoom;
        state.State.CurrentRoom = roomNumber;
        await state.WriteStateAsync();

        logger.LogInformation("User {UserId} changed room from {OldRoom} to {NewRoom}",
            this.GetPrimaryKeyString(), oldRoom, roomNumber);
    }
}
 
public class UserState
{
    public int LoginCount { get; set; } = 0;
    public int CurrentRoom { get; set; } = 1;
}