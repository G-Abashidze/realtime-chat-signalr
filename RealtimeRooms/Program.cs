using RealtimeRooms.Components;
using RealtimeRooms.Hubs;
using RealtimeRooms.Services;
using RealtimeRooms.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Add SignalR
builder.Services.AddSignalR();

// Add HTTP client for the API service
builder.Services.AddHttpClient();

// Add room store
builder.Services.AddSingleton<IRoomStore, InMemoryRoomStore>();

// Add client services for server-side rendering
builder.Services.AddScoped<RealtimeRooms.Client.Services.HubClientService>();
builder.Services.AddScoped<RealtimeRooms.Client.Services.RoomsApiService>();
builder.Services.AddScoped<RealtimeRooms.Client.Services.AppStateService>();

// Add CORS for WebAssembly client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm", policy =>
    {
        policy.WithOrigins("https://localhost:5143", "http://localhost:5143", "https://localhost:5001", "http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// Enable CORS
app.UseCors("AllowBlazorWasm");

// Map SignalR hub
app.MapHub<ChatHub>("/hubs/chat");

// Map API endpoints
app.MapGet("/api/rooms", async (IRoomStore roomStore) =>
{
    return await roomStore.GetRoomsAsync();
});

app.MapPost("/api/rooms", async (CreateRoomRequest request, IRoomStore roomStore) =>
{
    var roomId = await roomStore.CreateRoomAsync(request.Name);
    return new CreateRoomResponse { RoomId = roomId };
});

app.MapGet("/api/rooms/{roomId}/history", async (string roomId, IRoomStore roomStore) =>
{
    return await roomStore.GetMessageHistoryAsync(roomId);
});

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(RealtimeRooms.Client._Imports).Assembly);

app.Run();
