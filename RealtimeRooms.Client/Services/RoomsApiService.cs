using System.Text.Json;
using RealtimeRooms.Shared.Models;

namespace RealtimeRooms.Client.Services;

/// <summary>
/// API service for room management
/// </summary>
public class RoomsApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public RoomsApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Get all available rooms
    /// </summary>
    public async Task<List<RoomInfo>> GetRoomsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/rooms");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<RoomInfo>>(json, _jsonOptions) ?? new List<RoomInfo>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting rooms: {ex.Message}");
            return new List<RoomInfo>();
        }
    }

    /// <summary>
    /// Create a new room
    /// </summary>
    public async Task<string?> CreateRoomAsync(string name)
    {
        try
        {
            var request = new CreateRoomRequest { Name = name };
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/rooms", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CreateRoomResponse>(responseJson, _jsonOptions);
            
            return result?.RoomId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating room: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get message history for a room
    /// </summary>
    public async Task<List<ChatMessage>> GetMessageHistoryAsync(string roomId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/rooms/{roomId}/history");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ChatMessage>>(json, _jsonOptions) ?? new List<ChatMessage>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting message history: {ex.Message}");
            return new List<ChatMessage>();
        }
    }
}
