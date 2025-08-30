using System.Collections.Concurrent;
using RealtimeRooms.Shared.Models;

namespace RealtimeRooms.Services;

/// <summary>
/// Represents the state of a room
/// </summary>
public class RoomState
{
    public string RoomId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ConcurrentDictionary<string, UserSummary> Participants { get; } = new();
    
    // Circular buffer for last 50 messages
    private readonly Queue<ChatMessage> _messages = new();
    private readonly object _messagesLock = new();
    private const int MaxMessages = 50;

    public void AddMessage(ChatMessage message)
    {
        lock (_messagesLock)
        {
            _messages.Enqueue(message);
            while (_messages.Count > MaxMessages)
            {
                _messages.Dequeue();
            }
        }
    }

    public List<ChatMessage> GetMessages()
    {
        lock (_messagesLock)
        {
            return _messages.ToList();
        }
    }
}

/// <summary>
/// In-memory implementation of room store
/// </summary>
public class InMemoryRoomStore : IRoomStore
{
    private readonly ConcurrentDictionary<string, RoomState> _rooms = new();

    public Task<string> CreateRoomAsync(string name)
    {
        var roomId = Guid.NewGuid().ToString();
        var roomState = new RoomState
        {
            RoomId = roomId,
            Name = name
        };
        
        _rooms.TryAdd(roomId, roomState);
        return Task.FromResult(roomId);
    }

    public Task<List<RoomInfo>> GetRoomsAsync()
    {
        var rooms = _rooms.Values.Select(r => new RoomInfo
        {
            RoomId = r.RoomId,
            Name = r.Name,
            MemberCount = r.Participants.Count
        }).ToList();
        
        return Task.FromResult(rooms);
    }

    public Task<bool> DeleteRoomAsync(string roomId)
    {
        return Task.FromResult(_rooms.TryRemove(roomId, out _));
    }

    public Task<bool> AddParticipantAsync(string roomId, UserSummary user)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            room.Participants.TryAdd(user.UserId, user);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> RemoveParticipantAsync(string roomId, string userId)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            return Task.FromResult(room.Participants.TryRemove(userId, out _));
        }
        return Task.FromResult(false);
    }

    public Task<bool> SetTypingAsync(string roomId, string userId, bool isTyping)
    {
        if (_rooms.TryGetValue(roomId, out var room) && 
            room.Participants.TryGetValue(userId, out var user))
        {
            user.IsTyping = isTyping;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> AddMessageAsync(ChatMessage message)
    {
        if (_rooms.TryGetValue(message.RoomId, out var room))
        {
            room.AddMessage(message);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<List<ChatMessage>> GetMessageHistoryAsync(string roomId)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            return Task.FromResult(room.GetMessages());
        }
        return Task.FromResult(new List<ChatMessage>());
    }

    public Task<RoomPresence?> GetPresenceAsync(string roomId)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            var presence = new RoomPresence
            {
                RoomId = roomId,
                Users = room.Participants.Values.ToList()
            };
            return Task.FromResult<RoomPresence?>(presence);
        }
        return Task.FromResult<RoomPresence?>(null);
    }

    public Task<bool> RoomExistsAsync(string roomId)
    {
        return Task.FromResult(_rooms.ContainsKey(roomId));
    }
}
