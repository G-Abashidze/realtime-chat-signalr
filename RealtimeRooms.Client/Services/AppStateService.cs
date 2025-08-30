using RealtimeRooms.Shared.Models;

namespace RealtimeRooms.Client.Services;

/// <summary>
/// Application state management service
/// </summary>
public class AppStateService
{
    private string? _displayName;
    private string? _currentUserId;
    private string? _currentRoomId;
    private ConnectionState _connectionState = ConnectionState.Disconnected;
    private readonly List<ChatMessage> _messages = new();
    private readonly List<UserSummary> _currentUsers = new();

    // Events for state changes
    public event Action? OnStateChanged;

    /// <summary>
    /// Current user's display name
    /// </summary>
    public string? DisplayName
    {
        get => _displayName;
        set
        {
            _displayName = value;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Current user's ID
    /// </summary>
    public string? CurrentUserId
    {
        get => _currentUserId;
        set
        {
            _currentUserId = value;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Current room ID
    /// </summary>
    public string? CurrentRoomId
    {
        get => _currentRoomId;
        set
        {
            _currentRoomId = value;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Current connection state
    /// </summary>
    public ConnectionState ConnectionState
    {
        get => _connectionState;
        set
        {
            _connectionState = value;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Current messages in the room
    /// </summary>
    public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();

    /// <summary>
    /// Current users in the room
    /// </summary>
    public IReadOnlyList<UserSummary> CurrentUsers => _currentUsers.AsReadOnly();

    /// <summary>
    /// Add a message to the current room
    /// </summary>
    public void AddMessage(ChatMessage message)
    {
        _messages.Add(message);
        NotifyStateChanged();
    }

    /// <summary>
    /// Set the message history for the current room
    /// </summary>
    public void SetMessageHistory(List<ChatMessage> messages)
    {
        _messages.Clear();
        _messages.AddRange(messages);
        NotifyStateChanged();
    }

    /// <summary>
    /// Clear all messages
    /// </summary>
    public void ClearMessages()
    {
        _messages.Clear();
        NotifyStateChanged();
    }

    /// <summary>
    /// Update the presence (current users) for the room
    /// </summary>
    public void UpdatePresence(RoomPresence presence)
    {
        _currentUsers.Clear();
        _currentUsers.AddRange(presence.Users);
        NotifyStateChanged();
    }

    /// <summary>
    /// Update typing status for a user
    /// </summary>
    public void UpdateTypingStatus(string userId, bool isTyping)
    {
        var user = _currentUsers.FirstOrDefault(u => u.UserId == userId);
        if (user != null)
        {
            user.IsTyping = isTyping;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Clear all state (for leaving room or disconnecting)
    /// </summary>
    public void ClearRoomState()
    {
        _currentRoomId = null;
        _currentUserId = null;
        _messages.Clear();
        _currentUsers.Clear();
        NotifyStateChanged();
    }

    /// <summary>
    /// Get users who are currently typing (excluding current user display name)
    /// </summary>
    public List<string> GetTypingUsers()
    {
        return _currentUsers
            .Where(u => u.IsTyping && u.DisplayName != _displayName)
            .Select(u => u.DisplayName)
            .ToList();
    }

    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}
