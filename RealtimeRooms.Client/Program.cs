using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RealtimeRooms.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add HTTP client for API calls
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add application services
builder.Services.AddScoped<HubClientService>();
builder.Services.AddScoped<RoomsApiService>();
builder.Services.AddScoped<AppStateService>();

await builder.Build().RunAsync();
