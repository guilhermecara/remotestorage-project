using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using remotestorage.Components;
using remotestorage.AuthenticationService;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

var builder = WebApplication.CreateBuilder(args);

// === CONFIGURATION ===
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var apiBaseUrl = Environment.GetEnvironmentVariable("API_DOCKER_URL")
                 ?? builder.Configuration["API_BASE_URL"];

// === SERVICES ===
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<UIState>();
builder.Services.AddHttpContextAccessor();

// === AUTHENTICATION ===
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"]
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["auth_token"];
                if (!string.IsNullOrEmpty(token))
                    context.Token = token;
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.Redirect("/login");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddRouting();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

// === BLAZOR SETUP ===
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRazorPages();

// === API CLIENT ===
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseCookies = true,
    CookieContainer = new CookieContainer()
});

var app = builder.Build();

// === MIDDLEWARE PIPELINE ===
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["auth_token"];
    var user = context.User.Identity;

    Console.WriteLine("=== AUTH DEBUG ===");
    Console.WriteLine($"Path: {context.Request.Path}");
    Console.WriteLine($"Cookie present: {token != null}");
    Console.WriteLine($"User authenticated: {user?.IsAuthenticated}");
    Console.WriteLine($"User name: {user?.Name ?? "null"}");
    Console.WriteLine($"Claims: {string.Join(" | ", context.User.Claims.Select(c => $"{c.Type}={c.Value}"))}");
    Console.WriteLine("==================");

    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    switch (response.StatusCode)
    {
        case 404:
            response.Redirect("/notfound");
            break;
    }
});

// === BLAZOR MAP ===
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
