namespace WanasChat.Interfaces;

[Alias("IUserGrain")]
public interface IUserGrain : IGrainWithStringKey
{
    [Alias("GetLoginCount")]
    Task<int> GetLoginCount();
    [Alias("IncrementLoginCount")]
    Task IncrementLoginCount();
    [Alias("GetCurrentRoom")]
    Task<int> GetCurrentRoom();
    [Alias("SetCurrentRoom")]
    Task SetCurrentRoom(int roomNumber);
}