namespace RealtimeRooms.Shared.Models;

/// <summary>
/// Represents a chat message in a room
/// </summary>
public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string RoomId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset SentAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public bool IsSystemMessage { get; set; } = false;
}

/// <summary>
/// Summary information about a user in a room
/// </summary>
public class UserSummary
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsTyping { get; set; } = false;
}

/// <summary>
/// Basic information about a room
/// </summary>
public class RoomInfo
{
    public string RoomId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MemberCount { get; set; } = 0;
}

/// <summary>
/// Complete presence information for a room
/// </summary>
public class RoomPresence
{
    public string RoomId { get; set; } = string.Empty;
    public List<UserSummary> Users { get; set; } = new();
}

/// <summary>
/// Request to create a new room
/// </summary>
public class CreateRoomRequest
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Response after creating a room
/// </summary>
public class CreateRoomResponse
{
    public string RoomId { get; set; } = string.Empty;
}

/// <summary>
/// Connection states for the SignalR client
/// </summary>
public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting
}
