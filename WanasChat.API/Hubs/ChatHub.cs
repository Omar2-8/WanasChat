using Microsoft.AspNetCore.SignalR;
using WanasChat.Interfaces;

namespace WanasChat.API.Hubs;

public class ChatHub(IClusterClient clusterClient, ILogger<ChatHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var username = Context.GetHttpContext()?.Request.Query["username"].ToString();
        if (string.IsNullOrEmpty(username))
        {
            username = $"User_{Context.ConnectionId}";
        }

        Context.Items["username"] = username;

        var userGrain = clusterClient.GetGrain<IUserGrain>(username);
        await userGrain.IncrementLoginCount();
        var loginCount = await userGrain.GetLoginCount();
        var currentRoom = await userGrain.GetCurrentRoom();

        var roomGrain = clusterClient.GetGrain<IRoomGrain>(currentRoom);
        await roomGrain.UserJoined(username);

        await Groups.AddToGroupAsync(Context.ConnectionId, currentRoom.ToString());

        await Clients.OthersInGroup(currentRoom.ToString()).SendAsync(
            ChatEvents.UserJoined,
            username,
            loginCount);

        logger.LogInformation("User {Username} connected with ID {ConnectionId}", username, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context.Items["username"] as string;
        if (!string.IsNullOrEmpty(username))
        {
            var userGrain = clusterClient.GetGrain<IUserGrain>(username);
            var currentRoom = await userGrain.GetCurrentRoom();

            var roomGrain = clusterClient.GetGrain<IRoomGrain>(currentRoom);
            await roomGrain.UserLeft(username);

            await Clients.OthersInGroup(currentRoom.ToString()).SendAsync(
                ChatEvents.UserLeft,
                username);

            logger.LogInformation("User {Username} disconnected", username);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task ChangeRoom(int roomNumber)
    {
        var username = Context.Items["username"] as string;
        if (string.IsNullOrEmpty(username))
        {
            throw new HubException("Username not found");
        }

        var userGrain = clusterClient.GetGrain<IUserGrain>(username);
        var currentRoom = await userGrain.GetCurrentRoom();

        if (currentRoom == roomNumber)
        {
            logger.LogInformation("User {Username} attempted to change to the same room {RoomNumber}", username, roomNumber);
            return;
        }

        var oldRoomGrain = clusterClient.GetGrain<IRoomGrain>(currentRoom);
        await oldRoomGrain.UserLeft(username);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, currentRoom.ToString());
        await Clients.OthersInGroup(currentRoom.ToString()).SendAsync(
            ChatEvents.UserLeft,
            username);

        await userGrain.SetCurrentRoom(roomNumber);
        var loginCount = await userGrain.GetLoginCount();
        var newRoomGrain = clusterClient.GetGrain<IRoomGrain>(roomNumber);
        await newRoomGrain.UserJoined(username);
        await Groups.AddToGroupAsync(Context.ConnectionId, roomNumber.ToString());
        await Clients.OthersInGroup(roomNumber.ToString()).SendAsync(
            ChatEvents.UserJoined,
            username,
            loginCount);

        var recentMessages = await newRoomGrain.GetRecentMessages(5);

        await Clients.Caller.SendAsync(
            ChatEvents.RoomChanged,
            roomNumber,
            recentMessages);

        logger.LogInformation("User {Username} changed from room {OldRoom} to {NewRoom}",
            username, currentRoom, roomNumber);
    }

    public async Task SendMessage(string message)
    {
        var username = Context.Items["username"] as string;
        if (string.IsNullOrEmpty(username))
        {
            throw new HubException("Username not found");
        }

        var userGrain = clusterClient.GetGrain<IUserGrain>(username);
        var currentRoom = await userGrain.GetCurrentRoom();
        var loginCount = await userGrain.GetLoginCount();

        var roomGrain = clusterClient.GetGrain<IRoomGrain>(currentRoom);
        await roomGrain.AddMessage(username, message, loginCount);

        var chatMessage = new ChatMessage(username, loginCount, message);
        await Clients.Group(currentRoom.ToString()).SendAsync(
            ChatEvents.ReceiveMessage,
            chatMessage);

        logger.LogInformation("User {Username} sent message in room {RoomNumber}: {Message}",
            username, currentRoom, message);
    }
}