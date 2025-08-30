using Microsoft.AspNetCore.SignalR.Client;
using RealtimeRooms.Shared.Models;

namespace RealtimeRooms.Client.Services;

/// <summary>
/// Service for managing SignalR connection and communication
/// </summary>
public class HubClientService : IDisposable
{
    private HubConnection? _hubConnection;
    private readonly ILogger<HubClientService> _logger;
    private bool _disposed = false;

    // Events for server notifications
    public event Action? OnConnected;
    public event Action? OnDisconnected;
    public event Action? OnReconnecting;
    public event Action? OnReconnected;
    public event Action<UserSummary>? OnUserJoined;
    public event Action<string>? OnUserLeft;
    public event Action<ChatMessage>? OnMessageReceived;
    public event Action<string, bool>? OnTypingUpdated;
    public event Action<RoomPresence>? OnPresenceUpdated;
    public event Action<string>? OnError;
    public event Action<string>? OnUserIdAssigned;

    public HubClientService(ILogger<HubClientService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Current connection state
    /// </summary>
    public ConnectionState ConnectionState => _hubConnection?.State switch
    {
        HubConnectionState.Disconnected => ConnectionState.Disconnected,
        HubConnectionState.Connecting => ConnectionState.Connecting,
        HubConnectionState.Connected => ConnectionState.Connected,
        HubConnectionState.Reconnecting => ConnectionState.Reconnecting,
        _ => ConnectionState.Disconnected
    };

    /// <summary>
    /// Connect to the SignalR hub
    /// </summary>
    public async Task ConnectAsync(string baseUrl)
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }

        // Ensure baseUrl doesn't end with slash and hub path doesn't start with slash
        var cleanBaseUrl = baseUrl.TrimEnd('/');
        var hubUrl = $"{cleanBaseUrl}/hubs/chat";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        // Register event handlers
        RegisterEventHandlers();

        try
        {
            await _hubConnection.StartAsync();
            _logger.LogInformation("Connected to SignalR hub");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to SignalR hub");
            throw;
        }
    }

    /// <summary>
    /// Disconnect from the hub
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }

    /// <summary>
    /// Join a room
    /// </summary>
    public async Task JoinRoomAsync(string roomId, string displayName)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("JoinRoom", roomId, displayName);
        }
    }

    /// <summary>
    /// Leave a room
    /// </summary>
    public async Task LeaveRoomAsync(string roomId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("LeaveRoom", roomId);
        }
    }

    /// <summary>
    /// Send a message to the room
    /// </summary>
    public async Task SendMessageAsync(string roomId, string text)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("SendMessage", roomId, text);
        }
    }

    /// <summary>
    /// Set typing status
    /// </summary>
    public async Task SetTypingAsync(string roomId, bool isTyping)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("SetTyping", roomId, isTyping);
        }
    }

    /// <summary>
    /// Register event handlers for SignalR events
    /// </summary>
    private void RegisterEventHandlers()
    {
        if (_hubConnection == null) return;

        _hubConnection.On("Connected", () =>
        {
            _logger.LogInformation("SignalR Connected event received");
            OnConnected?.Invoke();
        });

        _hubConnection.On<UserSummary>("UserJoined", (user) =>
        {
            _logger.LogInformation("User joined: {DisplayName}", user.DisplayName);
            OnUserJoined?.Invoke(user);
        });

        _hubConnection.On<string>("UserLeft", (userId) =>
        {
            _logger.LogInformation("User left: {UserId}", userId);
            OnUserLeft?.Invoke(userId);
        });

        _hubConnection.On<ChatMessage>("MessageReceived", (message) =>
        {
            _logger.LogInformation("Message received from {DisplayName}", message.DisplayName);
            OnMessageReceived?.Invoke(message);
        });

        _hubConnection.On<string, bool>("TypingUpdated", (userId, isTyping) =>
        {
            OnTypingUpdated?.Invoke(userId, isTyping);
        });

        _hubConnection.On<RoomPresence>("PresenceUpdated", (presence) =>
        {
            _logger.LogInformation("Presence updated for room {RoomId}", presence.RoomId);
            OnPresenceUpdated?.Invoke(presence);
        });

        _hubConnection.On<string>("Error", (message) =>
        {
            _logger.LogError("SignalR error: {Message}", message);
            OnError?.Invoke(message);
        });

        _hubConnection.On<string>("UserIdAssigned", (userId) =>
        {
            _logger.LogInformation("User ID assigned: {UserId}", userId);
            OnUserIdAssigned?.Invoke(userId);
        });

        _hubConnection.Closed += (error) =>
        {
            _logger.LogInformation("SignalR connection closed");
            OnDisconnected?.Invoke();
            return Task.CompletedTask;
        };

        _hubConnection.Reconnecting += (error) =>
        {
            _logger.LogInformation("SignalR reconnecting");
            OnReconnecting?.Invoke();
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += (connectionId) =>
        {
            _logger.LogInformation("SignalR reconnected with connection ID: {ConnectionId}", connectionId);
            OnReconnected?.Invoke();
            return Task.CompletedTask;
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _hubConnection?.DisposeAsync();
            _disposed = true;
        }
    }
}
