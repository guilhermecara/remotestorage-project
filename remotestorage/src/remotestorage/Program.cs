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

// === Authentication ===

/*
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)

    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // MAKE ALWAYS WHEN HTTPS IS IMPLEMENTED!
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/login";
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)
            )
        };
    });
*/

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // for local HTTP
        options.Cookie.Path = "/"; // 👈 ensure it's visible to the whole app
        options.LoginPath = "/login";
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

app.UseAuthentication();
app.UseAuthorization();



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

    var handler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]!);

    var principal = handler.ValidateToken(token, new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    }, out _);

    await ctx.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
        });

    return Results.Ok();
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


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.Use(async (context, next) =>
{
    // Log cookies to verify
    var cookies = context.Request.Headers.Cookie.ToString();
    Console.WriteLine($"Incoming cookies: {cookies}");
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();