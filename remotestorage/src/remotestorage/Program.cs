using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using remotestorage.Components;
using remotestorage.AuthenticationService;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var apiBaseUrl = Environment.GetEnvironmentVariable("API_DOCKER_URL")
                 ?? builder.Configuration["API_BASE_URL"]; // defined in appsettings.json, fallback if no docker

// Services
builder.Services.AddScoped<UIState>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<JwtService>();

// Register authentication provider
builder.Services.AddAuthenticationCore();
builder.Services.AddAuthenticationCore();
builder.Services.AddScoped<AuthenticationStateProvider, LocalStorageAuthStateProvider>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});


var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
