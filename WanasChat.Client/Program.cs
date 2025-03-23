using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using WanasChat.Interfaces;

namespace WanasChat.Client;

public class Program
{
    private static HubConnection? connection;
    private static string? _username; 
    private static readonly ManualResetEvent _exitEvent = new(false);

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Chat Client");
        Console.WriteLine("===========");

         
        if (args.Length > 0)
            _username = args[0];
        else
        {
            Console.Write("Enter your username: ");
            _username = Console.ReadLine();
        }

        string? serviceAddress;
        if (args.Length > 1)
        {
            serviceAddress = args[1];
        }
        else
        {
            Console.Write("Enter service address (default: http://localhost:5082): ");
            serviceAddress = Console.ReadLine();
            if (string.IsNullOrEmpty(serviceAddress))
            {
                serviceAddress = "http://localhost:5082";
            }
        }

        try
        {
            connection = new HubConnectionBuilder()
                .WithUrl($"{serviceAddress}/wanas?username={_username}")
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .WithAutomaticReconnect()
                .Build();

            connection.Closed += ClientHelper.OnConnectionClosed;
            connection.Reconnecting += ClientHelper.OnReconnecting;
            connection.Reconnected += ClientHelper.OnReconnected;

            connection.On<ChatMessage>(ChatEvents.ReceiveMessage, ClientHelper.OnReceiveMessage);
            connection.On<string, int>(ChatEvents.UserJoined, ClientHelper.OnUserJoined);
            connection.On<string>(ChatEvents.UserLeft, ClientHelper.OnUserLeft);
            connection.On<int, List<ChatMessage>>(ChatEvents.RoomChanged, ClientHelper.OnRoomChanged);

            await ClientHelper.ConnectWithRetry(connection);

            Console.WriteLine("Commands:");
            Console.WriteLine("  room [number] - Change room");
            Console.WriteLine("  send [message] - Send message");
            Console.WriteLine("  exit - Exit the client");
            Console.WriteLine();

            _ = Task.Run(() => ClientHelper.ProcessUserCommands(connection,_exitEvent));

            _exitEvent.WaitOne();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    } 
}