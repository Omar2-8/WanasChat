# Wanas Chat Application

A simple, scalable chat service built with ASP.NET Core 9, Microsoft Orleans 9, SignalR, and Redis.

## Overview

This application implements a real-time chat service with the following features:

- Room-based messaging system
- User login counting and tracking
- Persistent message storage in Redis
- Real-time notifications for user joins/leaves and messages

## Architecture

- **SignalR Hub**: Manages real-time communication between clients
- **Orleans Grains**: Handles business logic and state management
  - User Grain: Tracks user state (login count, current room)
  - Room Grain: Manages room messages and user membership
- **Redis**: Provides persistent storage for messages and user data

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/products/docker-desktop/) (optional)
- [Redis](https://redis.io/download) (can be run locally or via Docker)

## Project Structure

- **ChatService.Interfaces**: Contains grain interfaces and shared data models
- **ChatService.Grains**: Implements Orleans grains for user and room management
- **ChatService.API**: Hosts the SignalR hub and Orleans silo
- **ChatService.Client**: Console application for testing the chat service

## Setup Instructions

1. **Install .NET 9 SDK**
   
   Download and install from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/9.0)

2. **Install and Start Redis**
   
   - **Windows**: Download and run [Redis for Windows](https://github.com/tporadowski/redis/releases)
   - **Linux/macOS**: 
     ```bash
     # Using Docker
     docker run -p 6379:6379 redis
     
     # Or install via package manager (Linux)
     sudo apt-get install redis-server
     sudo systemctl start redis
     
     # macOS with Homebrew
     brew install redis
     brew services start redis
     ```

3. **Clone and Build the Project**
   ```bash
   git clone 
   cd WanasChat
   dotnet build
   ```

4. **Configure Redis Connection**
   
   Update `appsettings.json` in the ChatService.API project if your Redis instance is not running on the default localhost:6379:
   ```json
   "Redis": {
     "ConnectionString": "localhost:6379",
     "DatabaseNumber": 0
   }
   ```

5. **Run the API Service**
   ```bash
   cd ChatService.API
   dotnet run
   ```
   The service will start on https://localhost:7001 and http://localhost:5000 by default.

6. **Run the Client Application**
   
   In a new terminal:
   ```bash
   cd ChatService.Client
   dotnet run -- YourUsername http://localhost:5000
   ```

## Testing the Application

Once the client is running, you can use the following commands:

- `room [number]` - Change to a different chat room
- `send [message]` - Send a message to the current room
- `exit` - Exit the client application

### Example Chat Session

```
Enter your username: Omar
Connected to the chat service.
Commands:
  room [number] - Change room
  send [message] - Send message
  exit - Exit the client

room 1
* Changed to room 1
* No messages in this room yet.

send Hello world!
[Omar (1 logins)]: Hello world!

room 2
* Changed to room 2
* No messages in this room yet.

send This is room 2
[Omar (1 logins)]: This is room 2
```

### Multiple Clients

To test with multiple users, open additional terminals and run the client with different usernames:

```bash
dotnet run -- Ali http://localhost:5000
```

## Troubleshooting

### Common Issues

1. **Connection Refused to Redis**
   - Ensure Redis is running and accessible
   - Check firewall settings
   - Verify the connection string in appsettings.json 

### Logs

- The API service logs to the console by default
- For more detailed logs, modify the logging level in appsettings.json:
  ```json
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Orleans": "Debug"
    }
  }
  ```

## Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Microsoft Orleans Documentation](https://dotnet.github.io/orleans/)
- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction)
- [Redis Documentation](https://redis.io/documentation)
