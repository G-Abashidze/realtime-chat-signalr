# Realtime Rooms

A minimal real-time chat application built with .NET 8, ASP.NET Core SignalR, and Blazor WebAssembly.

## Features

- **Real-time messaging**: Instant message delivery using SignalR
- **Room-based chat**: Create and join chat rooms
- **Presence indicators**: See who's online and typing
- **Anonymous authentication**: No login required, just provide a display name
- **Auto-reconnection**: Automatic reconnection when connection is lost
- **Message history**: Last 50 messages are preserved per room
- **Responsive design**: Works on desktop and mobile devices

## Architecture

- **RealtimeRooms.Server**: ASP.NET Core server with SignalR hub
- **RealtimeRooms.Client**: Blazor WebAssembly client application
- **RealtimeRooms.Shared**: Shared models and DTOs

## Quick Start

1. **Clone and navigate to the project:**
   ```bash
   cd RealtimeRooms
   ```

2. **Run the application:**
   ```bash
   dotnet run
   ```

3. **Open your browser and navigate to:**
   - https://localhost:5001 (HTTPS)
   - http://localhost:5000 (HTTP)

4. **Start chatting:**
   - Enter your display name
   - Create a new room or join an existing one
   - Start messaging!

## Development

### Project Structure

```
RealtimeRooms/
├── RealtimeRooms.Server/       # ASP.NET Core server
│   ├── Hubs/ChatHub.cs        # SignalR hub
│   ├── Services/              # Room management services
│   └── Program.cs             # Server configuration
├── RealtimeRooms.Client/       # Blazor WebAssembly client
│   ├── Pages/                 # Razor pages
│   ├── Services/              # Client services
│   └── wwwroot/               # Static assets
└── RealtimeRooms.Shared/       # Shared models
    └── Models.cs              # DTOs and models
```

### Key Components

#### Server-side (SignalR Hub)

- **ChatHub**: Handles all real-time communication
  - `JoinRoom(roomId, displayName)`: Join a chat room
  - `LeaveRoom(roomId)`: Leave a chat room
  - `SendMessage(roomId, text)`: Send a message
  - `SetTyping(roomId, isTyping)`: Update typing status

#### Client-side Services

- **HubClientService**: Manages SignalR connection and events
- **RoomsApiService**: REST API calls for room management
- **AppStateService**: Application state management

#### API Endpoints

- `GET /api/rooms`: List all available rooms
- `POST /api/rooms`: Create a new room
- `GET /api/rooms/{roomId}/history`: Get message history

### Real-time Events

The application surfaces these real-time notifications:

- **Connected**: Client successfully connected to hub
- **User joined/left**: Presence updates when users enter/leave rooms
- **Typing indicators**: Shows when users are typing (debounced)
- **Message delivery**: Real-time message broadcasting
- **Connection status**: Reconnecting/disconnected notifications

### Styling

The app uses minimal CSS without any UI frameworks:
- Responsive grid layout
- Clean typography
- Subtle animations for typing indicators
- Toast notifications for system events

## Browser Support

- Modern browsers with WebAssembly support
- Chrome, Firefox, Safari, Edge

## Technologies Used

- **.NET 8**: Latest version of .NET
- **ASP.NET Core**: Web framework for the server
- **SignalR**: Real-time communication library
- **Blazor WebAssembly**: Client-side web framework
- **CSS3**: Modern CSS features for styling
- **JavaScript**: Minimal JS for DOM interactions

## Future Enhancements

- Room-specific routing (`/rooms/{id}`)
- Rate limiting for message sending
- Unit tests for core functionality
- Message persistence with a database
- User authentication and authorization
- File sharing and rich media support
