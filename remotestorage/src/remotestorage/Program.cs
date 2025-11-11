using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using remotestorage.Components;
using remotestorage.AuthenticationService;
using remotestorage.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;        // ← para PostAsJsonAsync
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;    // ← para NavigationManager (se usar)
using Microsoft.Extensions.Http;          // ← se estiver configurando HttpClientFactory
using Microsoft.Extensions.DependencyInjection; // ← para AddHttpClient / CreateClient


var builder = WebApplication.CreateBuilder(args);

// Config
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var apiBaseUrl = Environment.GetEnvironmentVariable("API_DOCKER_URL")
                 ?? builder.Configuration["API_BASE_URL"];

// === SERVICES ===
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<UIState>();
builder.Services.AddHttpContextAccessor();

// === Authentication ===

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = apiBaseUrl,          
            ValidAudience = builder.Configuration["Jwt:Audience"],      
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };

        // Important for Blazor Server: pass token via SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["auth_token"];  // your cookie name
                if (!string.IsNullOrEmpty(token))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

// === Blazor Setup ===
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// === API Client ===
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

var app = builder.Build();

// === Middleware ===
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}


/*
// 3. Endpoint de login (CORRETO)
app.MapPost("/auth/set-cookie", async (HttpContext ctx, IHttpClientFactory httpFactory, IConfiguration config) =>
{
    var loginRequest = await ctx.Request.ReadFromJsonAsync<LoginRequest>();
    if (loginRequest == null) return Results.BadRequest();

    var client = httpFactory.CreateClient("API");
    var response = await client.PostAsJsonAsync("api/auth/login", new
    {
        loginRequest.Username,
        loginRequest.Password
    });

    if (!response.IsSuccessStatusCode) return Results.Unauthorized();

    var json = await response.Content.ReadFromJsonAsync<TokenResponse>();
    var token = json?.Token;
    if (string.IsNullOrEmpty(token)) return Results.Unauthorized();

    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddHours(12),
        IsEssential = true // opcional, evita bloqueio por consentimento
    };

    ctx.Response.Cookies.Append("AuthToken", token, cookieOptions);

    return Results.Ok(new { Message = "Auth cookie set successfully." });
})
.WithName("SetAuthCookie");

app.MapGet("/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    ctx.Response.Redirect("/login");
});

app.MapGet("/auth/whoami", (HttpContext ctx) =>
{
    if (ctx.User.Identity?.IsAuthenticated == true)
        return Results.Ok(new { user = ctx.User.Identity.Name });
    return Results.Unauthorized();
});
*/

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

/*
app.Use(async (context, next) =>
{
    // Log cookies to verify
    var cookies = context.Request.Headers.Cookie.ToString();
    Console.WriteLine($"Incoming cookies: {cookies}");
    await next();
});
*/
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapBlazorHub().RequireAuthorization();

app.Run();