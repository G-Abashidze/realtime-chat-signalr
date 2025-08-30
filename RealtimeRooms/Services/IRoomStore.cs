using RealtimeRooms.Shared.Models;

namespace RealtimeRooms.Services;

/// <summary>
/// Interface for managing rooms and their state
/// </summary>
public interface IRoomStore
{
    /// <summary>
    /// Create a new room
    /// </summary>
    Task<string> CreateRoomAsync(string name);
    
    /// <summary>
    /// Get all available rooms
    /// </summary>
    Task<List<RoomInfo>> GetRoomsAsync();
    
    /// <summary>
    /// Delete a room
    /// </summary>
    Task<bool> DeleteRoomAsync(string roomId);
    
    /// <summary>
    /// Add a participant to a room
    /// </summary>
    Task<bool> AddParticipantAsync(string roomId, UserSummary user);
    
    /// <summary>
    /// Remove a participant from a room
    /// </summary>
    Task<bool> RemoveParticipantAsync(string roomId, string userId);
    
    /// <summary>
    /// Update typing status for a user
    /// </summary>
    Task<bool> SetTypingAsync(string roomId, string userId, bool isTyping);
    
    /// <summary>
    /// Add a message to the room history (keeps last 50)
    /// </summary>
    Task<bool> AddMessageAsync(ChatMessage message);
    
    /// <summary>
    /// Get message history for a room (last 50 messages)
    /// </summary>
    Task<List<ChatMessage>> GetMessageHistoryAsync(string roomId);
    
    /// <summary>
    /// Get current presence information for a room
    /// </summary>
    Task<RoomPresence?> GetPresenceAsync(string roomId);
    
    /// <summary>
    /// Check if a room exists
    /// </summary>
    Task<bool> RoomExistsAsync(string roomId);
}
