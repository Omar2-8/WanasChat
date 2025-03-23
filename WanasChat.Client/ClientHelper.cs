using Microsoft.AspNetCore.SignalR.Client;
using WanasChat.Interfaces;

namespace WanasChat.Client;
public static class ClientHelper
{

    public static async Task ConnectWithRetry(HubConnection connection, int maxRetries = 5)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await connection!.StartAsync();
                Console.WriteLine("Connected to the chat service.");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect: {ex.Message}");
                if (i < maxRetries - 1)
                {
                    Console.WriteLine($"Retrying in {( i + 1 ) * 2} seconds...");
                    await Task.Delay(( i + 1 ) * 2000);
                }
                else
                {
                    Console.WriteLine("Could not connect to the server. Press any key to exit.");
                    Console.ReadKey();
                    Environment.Exit(1);
                }
            }
        }
    }

    public static async Task ProcessUserCommands(HubConnection connection, ManualResetEvent exitEvent)
    {
        while (true)
        {
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                await connection!.StopAsync();
                exitEvent.Set();
                break;
            }

            try
            {
                if (input.StartsWith("room ", StringComparison.OrdinalIgnoreCase))
                {
                    var roomPart = input[5..].Trim();
                    if (int.TryParse(roomPart, out int roomNumber) && roomNumber > 0)
                    {
                        await connection!.InvokeAsync("ChangeRoom", roomNumber);
                    }
                    else
                    {
                        Console.WriteLine("Invalid room number. Please enter a positive integer.");
                    }
                }
                else if (input.StartsWith("send ", StringComparison.OrdinalIgnoreCase))
                {
                    var message = input[5..].Trim();
                    if (!string.IsNullOrEmpty(message))
                    {
                        await connection!.InvokeAsync("SendMessage", message);
                    }
                }
                else
                {
                    Console.WriteLine("Unknown command. Available commands: room [number], send [message], exit");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    public static Task OnConnectionClosed(Exception? exception)
    {
        Console.WriteLine("Connection closed.");
        if (exception != null)
            Console.WriteLine($"Connection closed due to error: {exception.Message}");
        return Task.CompletedTask;
    }

    public static Task OnReconnecting(Exception? exception)
    {
        if (exception is not null)
            Console.WriteLine($"[error] Exception Occurred : {exception}");

        Console.WriteLine("Attempting to reconnect...");
        return Task.CompletedTask;
    }

    public static Task OnReconnected(string? connectionId)
    {
        if (connectionId is not null)
            Console.WriteLine($"Reconnected to the server., connection Id :{connectionId}");
        else
            Console.WriteLine("Reconnected to the server.");
        return Task.CompletedTask;
    }

    public static void OnReceiveMessage(ChatMessage message) => Console.WriteLine($"[{message.Username} ({message.LoginCount} logins)]: {message.Message}");

    public static void OnUserJoined(string username, int loginCount) => Console.WriteLine($"* {username} ({loginCount} logins) joined the room");

    public static void OnUserLeft(string username) => Console.WriteLine($"* {username} left the room");

    public static void OnRoomChanged(int roomNumber, List<ChatMessage> recentMessages)
    {
        Console.WriteLine($"* Changed to room {roomNumber}");
        if (recentMessages.Count > 0)
        {
            Console.WriteLine($"* Last {recentMessages.Count} messages in this room:");
            foreach (var message in recentMessages)
                Console.WriteLine($"  [{message.Username} ({message.LoginCount} logins)]: {message.Message}");
        }
        else
            Console.WriteLine($"* No messages in this room yet.");
    }
}
