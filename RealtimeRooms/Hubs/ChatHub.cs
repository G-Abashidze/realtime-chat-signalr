using Microsoft.AspNetCore.SignalR;
using RealtimeRooms.Services;
using RealtimeRooms.Shared.Models;
using System.Collections.Concurrent;

namespace RealtimeRooms.Hubs;

/// <summary>
/// SignalR Hub for real-time chat functionality
/// </summary>
public class ChatHub : Hub
{
    private readonly IRoomStore _roomStore;
    private readonly ILogger<ChatHub> _logger;
    
    // Track connection ID to user mapping
    private static readonly ConcurrentDictionary<string, UserConnection> _connections = new();

    public ChatHub(IRoomStore roomStore, ILogger<ChatHub> logger)
    {
        _roomStore = roomStore;
        _logger = logger;
    }

    /// <summary>
    /// Join a room with the specified display name
    /// </summary>
    public async Task JoinRoom(string roomId, string displayName)
    {
        try
        {
            if (!await _roomStore.RoomExistsAsync(roomId))
            {
                await Clients.Caller.SendAsync("Error", "Room does not exist");
                return;
            }

            var userId = Guid.NewGuid().ToString();
            var user = new UserSummary
            {
                UserId = userId,
                DisplayName = displayName,
                IsTyping = false
            };

            // Track this connection
            _connections[Context.ConnectionId] = new UserConnection
            {
                UserId = userId,
                DisplayName = displayName,
                RoomId = roomId
            };

            // Add to SignalR group
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            // Add to room store
            await _roomStore.AddParticipantAsync(roomId, user);

            // Notify room of new user
            await Clients.Group(roomId).SendAsync("UserJoined", user);

            // Send system message
            var systemMessage = new ChatMessage
            {
                RoomId = roomId,
                UserId = "system",
                DisplayName = "System",
                Text = $"{displayName} joined the room",
                IsSystemMessage = true
            };
            await _roomStore.AddMessageAsync(systemMessage);
            await Clients.Group(roomId).SendAsync("MessageReceived", systemMessage);

            // Send updated presence
            var presence = await _roomStore.GetPresenceAsync(roomId);
            if (presence != null)
            {
                await Clients.Group(roomId).SendAsync("PresenceUpdated", presence);
                await Clients.Caller.SendAsync("PresenceUpdated", presence);
            }

            // Send the user's ID back to them
            await Clients.Caller.SendAsync("UserIdAssigned", userId);

            _logger.LogInformation("User {DisplayName} joined room {RoomId}", displayName, roomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining room {RoomId}", roomId);
            await Clients.Caller.SendAsync("Error", "Failed to join room");
        }
    }

    /// <summary>
    /// Leave the current room
    /// </summary>
    public async Task LeaveRoom(string roomId)
    {
        try
        {
            if (_connections.TryGetValue(Context.ConnectionId, out var connection))
            {
                await RemoveUserFromRoom(connection, roomId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving room {RoomId}", roomId);
        }
    }

    /// <summary>
    /// Send a message to the room
    /// </summary>
    public async Task SendMessage(string roomId, string text)
    {
        try
        {
            if (!_connections.TryGetValue(Context.ConnectionId, out var connection))
            {
                await Clients.Caller.SendAsync("Error", "Not connected to any room");
                return;
            }

            if (connection.RoomId != roomId)
            {
                await Clients.Caller.SendAsync("Error", "Not in the specified room");
                return;
            }

            var message = new ChatMessage
            {
                RoomId = roomId,
                UserId = connection.UserId,
                DisplayName = connection.DisplayName,
                Text = text,
                IsSystemMessage = false
            };

            // Add to store
            await _roomStore.AddMessageAsync(message);

            // Broadcast to room
            await Clients.Group(roomId).SendAsync("MessageReceived", message);

            // Stop typing for this user
            await _roomStore.SetTypingAsync(roomId, connection.UserId, false);
            await Clients.Group(roomId).SendAsync("TypingUpdated", connection.UserId, false);

            _logger.LogInformation("Message sent by {DisplayName} in room {RoomId}", connection.DisplayName, roomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to room {RoomId}", roomId);
            await Clients.Caller.SendAsync("Error", "Failed to send message");
        }
    }

    /// <summary>
    /// Set typing status for the current user
    /// </summary>
    public async Task SetTyping(string roomId, bool isTyping)
    {
        try
        {
            if (!_connections.TryGetValue(Context.ConnectionId, out var connection))
            {
                return;
            }

            if (connection.RoomId != roomId)
            {
                return;
            }

            await _roomStore.SetTypingAsync(roomId, connection.UserId, isTyping);
            await Clients.Group(roomId).SendAsync("TypingUpdated", connection.UserId, isTyping);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating typing status for room {RoomId}", roomId);
        }
    }

    /// <summary>
    /// Handle client connection
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await Clients.Caller.SendAsync("Connected");
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Handle client disconnection
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (_connections.TryRemove(Context.ConnectionId, out var connection))
            {
                await RemoveUserFromRoom(connection, connection.RoomId);
                _logger.LogInformation("User {DisplayName} disconnected from room {RoomId}", 
                    connection.DisplayName, connection.RoomId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling disconnect for {ConnectionId}", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Helper method to remove user from room
    /// </summary>
    private async Task RemoveUserFromRoom(UserConnection connection, string roomId)
    {
        // Remove from SignalR group
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

        // Remove from room store
        await _roomStore.RemoveParticipantAsync(roomId, connection.UserId);

        // Send system message
        var systemMessage = new ChatMessage
        {
            RoomId = roomId,
            UserId = "system",
            DisplayName = "System",
            Text = $"{connection.DisplayName} left the room",
            IsSystemMessage = true
        };
        await _roomStore.AddMessageAsync(systemMessage);
        await Clients.Group(roomId).SendAsync("MessageReceived", systemMessage);

        // Notify room of user leaving
        await Clients.Group(roomId).SendAsync("UserLeft", connection.UserId);

        // Send updated presence
        var presence = await _roomStore.GetPresenceAsync(roomId);
        if (presence != null)
        {
            await Clients.Group(roomId).SendAsync("PresenceUpdated", presence);
        }
    }
}

/// <summary>
/// Represents a user connection
/// </summary>
public class UserConnection
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
}
